using System;
using System.Linq;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Gameplay.Shapes;
using FullPotential.Api.Gameplay.Targeting;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
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
            IFighter sourceFighter,
            ItemBase itemUsed,
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
            var itemIsWeapon = itemUsed is Weapon;

            if (!itemHasEffects || itemIsWeapon)
            {
                var targetFighter = target.GetComponent<IFighter>();

                if (targetFighter == null)
                {
                    //Debug.LogWarning("Target is not an IFighter. Target was: " + target);

                    //todo: zzz v0.5 - make SpawnBulletHole a VisualsBehaviour on BulletTrail
                    if (itemIsWeapon)
                    {
                        var registryType = (IWeapon)itemUsed.RegistryType;
                        var isRanged = registryType?.Category == WeaponCategory.Ranged;
                        if (isRanged)
                        {
                            SpawnBulletHole(target, position);
                        }
                    }

                    return;
                }

                //Debug.Log($"Applying just damage (no effects) to {targetFighter.FighterName}");

                var damageToDeal = GetDamageValueFromAttack(sourceFighter, itemUsed, targetFighter.GetDefenseValue()) * -1 * effectPercentage;

                var sourceFighterCriticalHitChance = sourceFighter.GetCriticalHitChance();
                var criticalTestValue = _random.Next(0, 101);
                var isCritical = criticalTestValue <= sourceFighterCriticalHitChance;

                if (isCritical)
                {
                    //Debug.Log($"CRITICAL! Chance:{sourceFighterCriticalHitChance}, test:{criticalTestValue}");

                    damageToDeal *= 2;
                }

                targetFighter.TakeDamageFromFighter(sourceFighter, itemUsed, position, (int)damageToDeal, isCritical);

                if (!itemIsWeapon)
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
                    var sideEffect = _typeRegistry.GetEffect(withSideEffect.SideEffectType);
                    ApplyEffect(null, sideEffect, itemUsed, sourceFighter.GameObject, position, effectPercentage);
                }
            }
        }

        private void SpawnBulletHole(
            GameObject target,
            Vector3? position)
        {
            //todo: zzz v0.4.1 - this should be client-side only
            //todo: zzz v0.5 - SpawnBulletHole only works for box colliders that line up with the X and Z alias

            if (!position.HasValue)
            {
                return;
            }

            var targetCollider = target.GetComponent<BoxCollider>();

            if (targetCollider == null)
            {
                return;
            }

            var vertices = targetCollider.GetBoxColliderVertices();

            var matchesX = vertices.Where(v => Mathf.Approximately(v.x, position.Value.x)).ToList();
            var matchesZ = vertices.Where(v => Mathf.Approximately(v.z, position.Value.z)).ToList();

            var points = matchesX.Count > 0
                ? matchesX
                : matchesZ;

            //Debug.DrawRay(points[0], Vector3.up, Color.cyan, 5);
            //Debug.DrawRay(points[1], Vector3.up, Color.cyan, 5);

            if (points.Count == 0)
            {
                return;
            }

            var vec1 = points[0] - position.Value;
            var vec2 = points[1] - position.Value;

            var norm = Vector3.Cross(vec1, vec2).normalized;

            var otherPoints = matchesX.Count > 0
                ? vertices.Where(v => Math.Abs(v.x - position.Value.x) > 0.1).ToList()
                : vertices.Where(v => Math.Abs(v.z - position.Value.z) > 0.1).ToList();

            var directionCheck = points[0] - otherPoints[0];

            if ((matchesX.Count > 0 && directionCheck.x > 0)
                || (matchesZ.Count > 0 && directionCheck.z > 0))
            {
                norm *= -1;
            }

            //Debug.DrawRay(position.Value, norm, Color.cyan, 5);

            var rotation = Quaternion.FromToRotation(-Vector3.forward, norm);

            var bulletHole = UnityEngine.Object.Instantiate(GameManager.Instance.Prefabs.Combat.BulletHole, position.Value, rotation);
            bulletHole.NetworkSpawn();
            UnityEngine.Object.Destroy(bulletHole, 5);
        }

        private bool IsEffectAllowed(ItemBase itemUsed, GameObject target, IEffect effect)
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

                    if (target.GetComponent<PlayerState>() != null)
                    {
                        //Debug.Log("Cannot target players");
                        return false;
                    }
                }
            }

            return true;
        }

        private void ApplyEffect(IFighter sourceFighter, IEffect effect, ItemBase itemUsed, GameObject targetGameObject, Vector3? position, float effectPercentage)
        {
            if (effect is IMovementEffect movementEffect)
            {
                ApplyMovementEffect(sourceFighter, itemUsed, movementEffect, targetGameObject, effectPercentage);
                return;
            }

            var targetFighter = targetGameObject.GetComponent<IFighter>();

            if (targetFighter == null)
            {
                //Debug.LogWarning($"Not applying {effect.TypeName} to {targetGameObject.name} because they are not an IFighter");
                return;
            }

            //Debug.Log($"Applying {effect.TypeName} to {targetFighter.FighterName}");

            switch (effect)
            {
                case IStatEffect statEffect:
                    ApplyStatEffect(targetFighter, statEffect, itemUsed, sourceFighter, position, effectPercentage);
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

        private void ApplyStatEffect(IFighter targetFighter, IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position, float effectPercentage)
        {
            switch (statEffect.AffectType)
            {
                case AffectType.PeriodicDecrease:
                case AffectType.PeriodicIncrease:
                    targetFighter.ApplyPeriodicActionToStat(statEffect, itemUsed, sourceFighter, effectPercentage);
                    return;

                case AffectType.SingleDecrease:
                case AffectType.SingleIncrease:
                    targetFighter.ApplyStatValueChange(statEffect, itemUsed, sourceFighter, position, effectPercentage);
                    return;

                case AffectType.TemporaryMaxDecrease:
                case AffectType.TemporaryMaxIncrease:
                    targetFighter.ApplyTemporaryMaxActionToStat(statEffect, itemUsed, sourceFighter, position, effectPercentage);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for affect {statEffect.AffectType}");
                    return;
            }
        }

        private void ApplyAttributeEffect(IFighter targetFighter, IAttributeEffect attributeEffect, ItemBase itemUsed, float effectPercentage)
        {
            targetFighter.AddAttributeModifier(attributeEffect, itemUsed, effectPercentage);
        }

        private void ApplyElementalEffect(IFighter targetFighter, IEffect elementalEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position, float effectPercentage)
        {
            targetFighter.ApplyElementalEffect(elementalEffect, itemUsed, sourceFighter, position, effectPercentage);
        }

        private void ApplyMaintainDistance(ItemBase itemUsed, GameObject targetGameObject, IFighter sourceFighter)
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

        private void ApplyMovementEffect(IFighter sourceFighter, ItemBase itemUsed, IMovementEffect movementEffect, GameObject targetGameObject, float effectPercentage)
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
            var force = itemUsed?.GetMovementForceValue(adjustForGravity)
                ?? ItemBase.GetHighInHighOutInRange(sourceFighter.GetAttributeValue(AffectableAttribute.Strength), 200, 500);

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

            targetRigidBody.AddForce(forceToApply, ForceMode.Acceleration);

            var nearbyClients = _rpcService.ForNearbyPlayersExcept(targetGameObject.transform.position, 0);
            targetMoveable.ApplyMovementForceClientRpc(forceToApply, ForceMode.Acceleration, nearbyClients);
        }

        public float AddVariationToValue(float basicValue)
        {
            var multiplier = (float)_random.Next(90, 111) / 100;
            var adder = _random.Next(0, 6);
            return (float)Math.Ceiling(basicValue / multiplier) + adder;
        }

        private int GetDamageValueFromAttack(IFighter sourceFighter, ItemBase itemUsed, int targetDefense, bool addVariation = true)
        {
            var weapon = itemUsed as Weapon;
            var weaponCategory = (weapon?.RegistryType as IWeapon)?.Category;

            float attackStrength = itemUsed?.Attributes.Strength ?? sourceFighter.GetAttributeValue(AffectableAttribute.Strength);
            var defenceRatio = 100f / (100 + targetDefense);

            if (weaponCategory == WeaponCategory.Ranged)
            {
                attackStrength /= weapon.GetBulletsPerSecond();
            }

            //Even a small attack can still do damage
            var damageDealtBasic = Mathf.Ceil(attackStrength * defenceRatio / ItemBase.StrengthDivisor);

            if (weapon != null && weapon.IsTwoHanded)
            {
                damageDealtBasic *= 2;
            }

            if (weaponCategory == WeaponCategory.Melee)
            {
                damageDealtBasic *= 2;
            }

            if (!addVariation)
            {
                return (int)damageDealtBasic;
            }

            return (int)AddVariationToValue(damageDealtBasic);
        }

        public int GetDamageValueFromAttack(IFighter sourceFighter, int targetDefense, bool addVariation = true)
        {
            return GetDamageValueFromAttack(sourceFighter, null, targetDefense, addVariation);
        }

        public int GetDamageValueFromAttack(ItemBase itemUsed, int targetDefense, bool addVariation = true)
        {
            return GetDamageValueFromAttack(null, itemUsed, targetDefense, addVariation);
        }

        public void SpawnTargetingGameObject(IFighter sourceFighter, Consumer consumer, Vector3 startPosition, Vector3 direction)
        {
            if (consumer.Targeting.NetworkPrefabAddress.IsNullOrWhiteSpace())
            {
                return;
            }

            _typeRegistry.LoadAddessable(
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

        public void SpawnShapeGameObject(IFighter sourceFighter, Consumer consumer, GameObject target, Vector3 fallbackPosition, Vector3 lookDirection)
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

            _typeRegistry.LoadAddessable(
                consumer.Shape.PrefabAddress,
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
