using System;
using System.Linq;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using FullPotential.Core.Registry.Effects.Movement;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class CombatService : ICombatService
    {
        private readonly System.Random _random = new System.Random();

        private readonly ITypeRegistry _typeRegistry;
        private readonly IRpcService _rpcService;
        private readonly Punch _punchEffect;

        public CombatService(
            ITypeRegistry typeRegistry,
            IRpcService rpcService)
        {
            _typeRegistry = typeRegistry;
            _rpcService = rpcService;
            
            _punchEffect = new Punch();
        }

        public void ApplyEffects(
            FighterBase sourceFighter,
            ItemForCombatBase itemUsed,
            GameObject target,
            Vector3? position,
            float effectPercentage
        )
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("ApplyEffects was not called on the server");
                return;
            }

            if (itemUsed == null)
            {
                ApplyMovementEffect(sourceFighter, null, _punchEffect, target, effectPercentage);
            }

            var itemHasEffects = itemUsed?.Effects != null && itemUsed.Effects.Any();
            var weapon = itemUsed as Weapon;

            if (!itemHasEffects || weapon != null)
            {
                var targetFighter = target.GetComponent<FighterBase>();

                if (targetFighter == null)
                {
                    return;
                }

                //Debug.Log($"Applying just damage (no effects) to {targetFighter.FighterName}");

                //todo: damage should be an effect, not its own separate thing
                var damageToDeal = GetDamageValueFromAttack(sourceFighter, itemUsed, targetFighter.GetDefenseValue()) * -1 * effectPercentage;

                var sourceFighterCriticalHitChance = sourceFighter.GetCriticalHitChance();
                var criticalTestValue = _random.Next(0, 101);
                var isCritical = criticalTestValue <= sourceFighterCriticalHitChance;

                if (isCritical)
                {
                    //Debug.Log($"CRITICAL! Chance:{sourceFighterCriticalHitChance}, test:{criticalTestValue}");

                    damageToDeal *= 2;
                }

                targetFighter.TriggerDamageDealtEvent((int)damageToDeal, sourceFighter, itemUsed, position, isCritical);

                if (weapon == null)
                {
                    return;
                }
            }

            if (itemUsed.Effects == null)
            {
                return;
            }

            foreach (var effect in itemUsed.Effects)
            {
                if (!IsEffectAllowed(itemUsed, target, effect))
                {
                    //Debug.Log($"Effect {effect.TypeName} is not permitted against target {target}");
                    continue;
                }

                //Debug.Log($"Applying effect {effect.TypeName} to {target.name}");

                ApplyEffect(sourceFighter, effect, itemUsed, target, position, effectPercentage);

                if (sourceFighter != null && effect is IHasSideEffect withSideEffect)
                {
                    var sideEffect = _typeRegistry.GetEffect(withSideEffect.SideEffectTypeId.ToString());
                    ApplyEffect(null, sideEffect, itemUsed, sourceFighter.GameObject, position, effectPercentage);
                }
            }
        }

        private bool IsEffectAllowed(ItemForCombatBase itemUsed, GameObject target, IEffect effect)
        {
            if (itemUsed is Consumer consumer
                && consumer.Targeting.IsContinuous)
            {
                if (effect is IMovementEffect movementEffect
                    && movementEffect.Direction == MovementDirection.MaintainDistance)
                {
                    if (target.GetComponent<MaintainDistance>() != null)
                    {
                        //Debug.Log("Continuing to hold target");
                        return false;
                    }

                    if (target.GetComponent<PlayerFighter>() != null)
                    {
                        //Debug.Log("Cannot target players");
                        return false;
                    }
                }
            }

            return true;
        }

        private void ApplyEffect(FighterBase sourceFighter, IEffect effect, ItemForCombatBase itemUsed, GameObject targetGameObject, Vector3? position, float effectPercentage)
        {
            if (effect is IMovementEffect movementEffect)
            {
                ApplyMovementEffect(sourceFighter, itemUsed, movementEffect, targetGameObject, effectPercentage);
                return;
            }

            var targetFighter = targetGameObject.GetComponent<FighterBase>();

            if (targetFighter == null)
            {
                //Debug.LogWarning($"Not applying {effect.TypeName} to {targetGameObject.name} because they are not an FighterBase");
                return;
            }

            //Debug.Log($"Applying {effect.TypeName} to {targetFighter.FighterName}");

            switch (effect)
            {
                case IResourceEffect resourceEffect:
                    ApplyResourceEffect(targetFighter, resourceEffect, itemUsed, sourceFighter, position, effectPercentage);
                    return;

                case IAttributeEffect attributeEffect:
                    ApplyAttributeEffect(targetFighter, attributeEffect, itemUsed, effectPercentage);
                    return;

                case IElement elementalEffect:
                    ApplyElementalEffect(targetFighter, elementalEffect, itemUsed, sourceFighter, position, effectPercentage);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for effect {effect}");
                    return;
            }
        }

        private void ApplyResourceEffect(FighterBase targetFighter, IResourceEffect resourceEffect, ItemForCombatBase itemUsed, FighterBase sourceFighter, Vector3? position, float effectPercentage)
        {
            switch (resourceEffect.AffectType)
            {
                case AffectType.PeriodicDecrease:
                case AffectType.PeriodicIncrease:
                    targetFighter.ApplyPeriodicActionToResource(resourceEffect, itemUsed, sourceFighter, effectPercentage);
                    return;

                case AffectType.SingleDecrease:
                case AffectType.SingleIncrease:
                    targetFighter.ApplyValueChangeToResource(resourceEffect, itemUsed, sourceFighter, position, effectPercentage);
                    return;

                case AffectType.TemporaryMaxDecrease:
                case AffectType.TemporaryMaxIncrease:
                    targetFighter.ApplyTemporaryMaxActionToResource(resourceEffect, itemUsed, sourceFighter, position, effectPercentage);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for affect {resourceEffect.AffectType}");
                    return;
            }
        }

        private void ApplyAttributeEffect(FighterBase targetFighter, IAttributeEffect attributeEffect, ItemForCombatBase itemUsed, float effectPercentage)
        {
            targetFighter.AddAttributeModifier(attributeEffect, itemUsed, effectPercentage);
        }

        private void ApplyElementalEffect(FighterBase targetFighter, IEffect elementalEffect, ItemForCombatBase itemUsed, FighterBase sourceFighter, Vector3? position, float effectPercentage)
        {
            targetFighter.ApplyElementalEffect(elementalEffect, itemUsed, sourceFighter, position, effectPercentage);
        }

        private void ApplyMaintainDistance(ItemForCombatBase itemUsed, GameObject targetGameObject, FighterBase sourceFighter)
        {
            if (itemUsed is not Consumer consumer || !consumer.Targeting.IsContinuous)
            {
                Debug.LogWarning("MaintainDistance has been incorrectly applied to an item");
                return;
            }

            var comp = targetGameObject.AddComponent<MaintainDistance>();
            comp.SourceFighter = sourceFighter;
            comp.Distance = (targetGameObject.transform.position - sourceFighter.Transform.position).magnitude;
            comp.Consumer = consumer;
        }

        private void ApplyMovementEffect(FighterBase sourceFighter, ItemForCombatBase itemUsed, IMovementEffect movementEffect, GameObject targetGameObject, float effectPercentage)
        {
            var targetRigidBody = targetGameObject.GetComponent<Rigidbody>();

            if (targetRigidBody == null)
            {
                //Debug.LogWarning($"Cannot move target '{targetGameObject.name}' as it does not have a RigidBody");
                return;
            }

            var targetLivingEntity = targetGameObject.GetComponent<LivingEntityBase>();

            if (targetLivingEntity != null)
            {
                targetLivingEntity.SetLastMover(sourceFighter);
            }

            if (movementEffect.Direction == MovementDirection.MaintainDistance)
            {
                ApplyMaintainDistance(itemUsed, targetGameObject, sourceFighter);
                return;
            }

            if (targetGameObject.GetComponent<NetworkObject>() == null)
            {
                Debug.LogWarning($"Cannot apply a movement effect to target '{targetGameObject.name}' as it does not have a NetworkObject component");
                return;
            }

            var targetMoveable = targetGameObject.GetComponent<IMoveable>();

            if (targetMoveable == null)
            {
                Debug.LogWarning($"Cannot apply a movement effect to target '{targetGameObject.name}' as it has no components that implement {nameof(IMoveable)}");
                return;
            }

            var adjustForGravity = movementEffect.Direction is MovementDirection.Up or MovementDirection.Down;
            var strength = itemUsed?.Attributes.Strength ?? sourceFighter.GetAttributeValue(AttributeAffected.Strength);
            var rawForce = ItemForCombatBase.GetHighInHighOutInRange(strength, 100, 300);

            var force = adjustForGravity
                ? rawForce * 2f
                : rawForce;

            force *= effectPercentage;

            Vector3 forceToApply;

            switch (movementEffect.Direction)
            {
                case MovementDirection.AwayFromSource:
                case MovementDirection.TowardSource:
                case MovementDirection.LeftFromSource:
                case MovementDirection.RightFromSource:

                    if (sourceFighter == null)
                    {
                        Debug.LogWarning("Attack source not found. Did they sign out?");
                        return;
                    }

                    if (movementEffect.Direction is MovementDirection.AwayFromSource or MovementDirection.TowardSource)
                    {
                        var sourcePosition = sourceFighter.RigidBody.transform.position;
                        var targetPosition = targetRigidBody.transform.position;

                        var forwardsBackwardsDirection = movementEffect.Direction == MovementDirection.AwayFromSource
                            ? targetPosition - sourcePosition
                            : sourcePosition - targetPosition;

                        //If targeting self
                        if (forwardsBackwardsDirection == Vector3.zero)
                        {
                            forwardsBackwardsDirection = -sourceFighter.RigidBody.transform.forward;
                        }

                        //Debug.Log($"Applying {force} force to {targetGameObject.name}");

                        forceToApply = forwardsBackwardsDirection.normalized * force;
                    }
                    else
                    {
                        var leftRightDirection = movementEffect.Direction == MovementDirection.LeftFromSource
                            ? -sourceFighter.RigidBody.transform.right
                            : sourceFighter.RigidBody.transform.right;

                        forceToApply = leftRightDirection * force;
                    }

                    break;

                case MovementDirection.Backwards:
                    forceToApply = -targetGameObject.transform.forward * force;
                    break;

                case MovementDirection.Forwards:
                    forceToApply = targetGameObject.transform.forward * force;
                    break;

                case MovementDirection.Down:
                    forceToApply = -targetGameObject.transform.up * force;
                    break;

                case MovementDirection.Up:
                    forceToApply = targetGameObject.transform.up * force;
                    break;

                default:
                    Debug.LogError($"Not implemented handling for movement direction {movementEffect.Direction}");
                    return;
            }

            const ForceMode forceMode = ForceMode.Force;

            targetRigidBody.AddForce(forceToApply, forceMode);

            var nearbyClients = _rpcService.ForNearbyPlayersExcept(targetGameObject.transform.position, 0);
            targetMoveable.ApplyMovementForceClientRpc(forceToApply, forceMode, nearbyClients);
        }

        public float AddVariationToValue(float basicValue)
        {
            var multiplier = (float)_random.Next(90, 111) / 100;
            var adder = _random.Next(0, 6);
            return (float)Math.Ceiling(basicValue / multiplier) + adder;
        }

        private int GetDamageValueFromAttack(FighterBase sourceFighter, ItemForCombatBase itemUsed, int targetDefense, bool addVariation = true)
        {
            var weapon = itemUsed as Weapon;

            float attackStrength = itemUsed?.Attributes.Strength ?? sourceFighter.GetAttributeValue(AttributeAffected.Strength);
            var defenceRatio = 100f / (100 + targetDefense);

            if (weapon != null && weapon.IsRanged)
            {
                attackStrength /= weapon.GetAmmoPerSecond();
            }

            //Even a small attack can still do damage
            var damageDealtBasic = Mathf.Ceil(attackStrength * defenceRatio / ItemForCombatBase.StrengthDivisor);

            if (weapon != null && weapon.IsTwoHanded)
            {
                damageDealtBasic *= 2;
            }

            if (weapon != null && weapon.IsMelee)
            {
                damageDealtBasic *= 2;
            }

            if (!addVariation)
            {
                return (int)damageDealtBasic;
            }

            return (int)AddVariationToValue(damageDealtBasic);
        }

        public int GetDamageValueFromAttack(FighterBase sourceFighter, int targetDefense, bool addVariation = true)
        {
            return GetDamageValueFromAttack(sourceFighter, null, targetDefense, addVariation);
        }

        public int GetDamageValueFromAttack(ItemForCombatBase itemUsed, int targetDefense, bool addVariation = true)
        {
            return GetDamageValueFromAttack(null, itemUsed, targetDefense, addVariation);
        }

        public void SpawnTargetingGameObject(FighterBase sourceFighter, Consumer consumer, Vector3 startPosition, Vector3 direction)
        {
            if (consumer.Targeting.NetworkPrefabAddress.IsNullOrWhiteSpace())
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                consumer.Targeting.NetworkPrefabAddress,
                prefab =>
                {
                    var targetingGameObject = UnityEngine.Object.Instantiate(prefab, startPosition, Quaternion.identity);

                    var targetingBehaviour = targetingGameObject.GetComponent<ITargetingBehaviour>();
                    targetingBehaviour.SourceFighter = sourceFighter;
                    targetingBehaviour.Consumer = consumer;
                    targetingBehaviour.Direction = direction;

                    targetingGameObject.NetworkSpawn();

                    if (consumer.Targeting.IsContinuous)
                    {
                        targetingGameObject.transform.parent = sourceFighter.Transform;
                    }

                    consumer.Stoppables.Add(new DestroyStoppable(targetingGameObject));
                });
        }

        public void SpawnShapeGameObject(FighterBase sourceFighter, Consumer consumer, GameObject target, Vector3 fallbackPosition, Vector3 lookDirection)
        {
            if (consumer.Shape == null)
            {
                return;
            }

            Vector3 spawnPosition;
            if (target == null || !target.CompareTagAny(Tags.Player, Tags.Enemy))
            {
                spawnPosition = fallbackPosition;
            }
            else
            {
                var pointUnderTarget = new Vector3(target.transform.position.x, -100, target.transform.position.z);
                var feetOfTarget = target.GetComponent<Collider>().ClosestPointOnBounds(pointUnderTarget);

                spawnPosition = Physics.Raycast(feetOfTarget, Vector3.down, out var hit)
                    ? hit.point
                    : fallbackPosition;
            }

            var rotation = Quaternion.LookRotation(lookDirection);
            rotation.x = 0;
            rotation.z = 0;

            _typeRegistry.LoadAddessable<GameObject>(
                consumer.Shape.NetworkPrefabAddress,
                prefab =>
                {
                    var shapeGameObject = UnityEngine.Object.Instantiate(prefab, spawnPosition, rotation);
                    var sceneService = GameManager.Instance.GetSceneBehaviour().GetSceneService();

                    shapeGameObject.transform.position = sceneService.GetHeightAdjustedPosition(spawnPosition, shapeGameObject);

                    shapeGameObject.layer = LayerMask.NameToLayer(Layers.NonSolid);

                    var shapeBehaviour = shapeGameObject.GetComponent<IShapeBehaviour>();
                    shapeBehaviour.SourceFighter = sourceFighter;
                    shapeBehaviour.Consumer = consumer;
                    shapeBehaviour.Direction = lookDirection;

                    shapeGameObject.NetworkSpawn();
                });
        }
    }
}
