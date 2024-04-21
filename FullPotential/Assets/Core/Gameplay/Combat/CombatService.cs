using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.CoreTypeIds;
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
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class CombatService : ICombatService
    {
        private readonly System.Random _random = new System.Random();

        private readonly ITypeRegistry _typeRegistry;
        private readonly IRpcService _rpcService;
        private readonly IMovementEffect _pushEffect;
        private readonly IEffect _hurtEffect;

        public CombatService(
            ITypeRegistry typeRegistry,
            IRpcService rpcService)
        {
            _typeRegistry = typeRegistry;
            _rpcService = rpcService;

            _pushEffect = typeRegistry.GetRegisteredByTypeId<IEffect>(EffectTypeIds.PushId) as IMovementEffect;
            _hurtEffect = typeRegistry.GetRegisteredByTypeId<IEffect>(EffectTypeIds.HurtId);
        }

        public void ApplyEffects(
            FighterBase sourceFighter,
            CombatItemBase itemUsed,
            GameObject target,
            Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("ApplyEffects was not called on the server");
                return;
            }

            if (itemUsed == null)
            {
                ApplyMovementEffect(sourceFighter, null, _pushEffect, target);
            }

            var itemUsedEffects = itemUsed?.Effects ?? new List<IEffect>();

            if (!itemUsedEffects.Any())
            {
                itemUsedEffects.Add(_hurtEffect);
            }

            foreach (var effect in itemUsedEffects)
            {
                if (!IsEffectAllowed(target, effect))
                {
                    //Debug.Log($"Effect {effect.TypeName} is not permitted against target {target}");
                    continue;
                }

                //Debug.Log($"Applying effect {effect.TypeName} to {target.name}");

                ApplyEffect(sourceFighter, itemUsed, effect, target, position);

                if (sourceFighter != null && effect is IHasSideEffect withSideEffect)
                {
                    var sideEffect = _typeRegistry.GetRegisteredByTypeId<IEffect>(withSideEffect.SideEffectTypeIdString);
                    ApplyEffect(null, itemUsed, sideEffect, sourceFighter.GameObject, position);
                }
            }
        }

        private static bool IsEffectAllowed(GameObject target, IEffect effect)
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

            return true;
        }

        private void ApplyEffect(FighterBase sourceFighter, CombatItemBase itemUsed, IEffect effect, GameObject targetGameObject, Vector3? position)
        {
            if (effect is IMovementEffect movementEffect)
            {
                ApplyMovementEffect(sourceFighter, itemUsed, movementEffect, targetGameObject);
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
                    ApplyResourceEffect(sourceFighter, itemUsed, resourceEffect, targetFighter, position);
                    return;

                case IAttributeEffect attributeEffect:
                    ApplyAttributeEffect(sourceFighter, itemUsed, attributeEffect, targetFighter, position);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for effect {effect}");
                    return;
            }
        }
        private float GetSourceCriticalHitChance(FighterBase sourceFighter)
        {
            var luckValue = sourceFighter.GetAttributeValue(AttributeAffected.Luck);

            if (luckValue < 20)
            {
                return 0;
            }

            //e.g. 50 luck would mean a 50/5 = 10% chance
            return luckValue / 5f;
        }

        public float GetEffectBaseChange(FighterBase sourceFighter, CombatItemBase itemUsed, IEffect effect, bool isCriticalHit)
        {
            var resourceEffect = effect as IResourceEffect;

            var isPeriodic = resourceEffect?.EffectActionType is EffectActionType.PeriodicDecrease or EffectActionType.PeriodicIncrease;

            float change;

            if (itemUsed == null)
            {
                if (sourceFighter != null)
                {
                    change = LivingEntityBase.GetAdjustedStrength(sourceFighter.GetAttributeValue(AttributeAffected.Strength));
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                change = isPeriodic
                    ? itemUsed.GetPeriodicEffectValueChange()
                    : itemUsed.GetSingleEffectValueChange();
            }

            var weapon = itemUsed as Weapon;

            if (weapon != null && weapon.IsRanged)
            {
                change /= weapon.GetAmmoPerSecond();
            }

            if (isCriticalHit)
            {
                change *= 2;
            }

            if (itemUsed != null && itemUsed.IsTwoHanded)
            {
                change *= 2;
            }

            if (weapon != null && weapon.IsMelee)
            {
                change *= 2;
            }

            return change;
        }

        public CombatResult GetCombatResult(FighterBase sourceFighter, CombatItemBase itemUsed, IEffect effect, LivingEntityBase targetLivingEntity)
        {
            var isCriticalHit = false;
            if (sourceFighter != null)
            {
                var sourceFighterCriticalHitChance = GetSourceCriticalHitChance(sourceFighter);
                var criticalTestValue = UnityEngine.Random.Range(0, 101);
                isCriticalHit = criticalTestValue <= sourceFighterCriticalHitChance;
            }

            var change = GetEffectBaseChange(sourceFighter, itemUsed, effect, isCriticalHit);

            var effectivePercentage = GetEffectivePercentage(targetLivingEntity);

            var adjustedChange = AddVariationToValue(change * effectivePercentage);

            switch (effect)
            {
                case IResourceEffect resourceEffect:
                {
                    if (resourceEffect.EffectActionType is EffectActionType.PeriodicDecrease or EffectActionType.SingleDecrease or EffectActionType.TemporaryMaxDecrease)
                    {
                        adjustedChange *= -1;
                    }

                    break;
                }
                case IAttributeEffect attributeEffect:
                {
                    if (!attributeEffect.IsTemporaryMaxIncrease)
                    {
                        adjustedChange *= -1;
                    }

                    break;
                }
            }

            return new CombatResult 
            { 
                Change = adjustedChange,
                IsCriticalHit = isCriticalHit
            };
        }

        private float GetEffectivePercentage(LivingEntityBase targetLivingEntity)
        {
            //todo: zzz v0.8 - replace with sum of Strength of items with resistance to Hurt

            var armorItems = _typeRegistry
                .GetRegisteredTypes<IArmor>()
                .Select(x => targetLivingEntity.Inventory.GetItemInSlot<Armor>(x.TypeId.ToString(), false));

            var sum = armorItems
                .Where(x => x != null)
                .Sum(a => a.Attributes.Strength);

            var targetDefenceValue = (int)Math.Floor((float)sum / armorItems.Count());

            var targetDefence = targetLivingEntity != null ? targetDefenceValue : 0;
            var defenceRatio = 100f / (100 + targetDefence);
            return defenceRatio;
        }

        private void ApplyResourceEffect(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffect resourceEffect, FighterBase targetFighter, Vector3? position)
        {
            switch (resourceEffect.EffectActionType)
            {
                case EffectActionType.PeriodicDecrease:
                case EffectActionType.PeriodicIncrease:
                    targetFighter.ApplyPeriodicActionToResource(sourceFighter, itemUsed, resourceEffect, position);
                    return;

                case EffectActionType.SingleDecrease:
                case EffectActionType.SingleIncrease:
                    targetFighter.ApplySingleValueChangeToResource(sourceFighter, itemUsed, resourceEffect, position);
                    return;

                case EffectActionType.TemporaryMaxDecrease:
                case EffectActionType.TemporaryMaxIncrease:
                    targetFighter.ApplyTemporaryMaxActionToResource(sourceFighter, itemUsed, resourceEffect, position);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for affect type {resourceEffect.EffectActionType}");
                    return;
            }
        }

        private void ApplyAttributeEffect(FighterBase sourceFighter, CombatItemBase itemUsed, IAttributeEffect attributeEffect, FighterBase targetFighter, Vector3? position)
        {
            var expiry = DateTime.Now.AddSeconds(itemUsed.GetEffectDuration());
            var attributeCombatResult = GetCombatResult(sourceFighter, itemUsed, attributeEffect, targetFighter);
            targetFighter.AddAttributeModifier(attributeEffect, attributeCombatResult.Change, expiry, position);
        }

        private void ApplyMaintainDistance(FighterBase sourceFighter, CombatItemBase itemUsed, GameObject targetGameObject)
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

        private void ApplyMovementEffect(FighterBase sourceFighter, CombatItemBase itemUsed, IMovementEffect movementEffect, GameObject targetGameObject)
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
                ApplyMaintainDistance(sourceFighter, itemUsed, targetGameObject);
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
            var rawForce = MathsHelper.GetHighInHighOutInRange(strength, 100, 300);

            var force = adjustForGravity
                ? rawForce * 2f
                : rawForce;

            //var effectPercentage = weaponInHand.IsDefensive ? Weapon.DefensiveWeaponDpsMultiplier : 1;

            //force *= effectPercentage;

            //todo: adjust value for immunities, e.g. players are immune to Hold

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

        private int AddVariationToValue(float basicValue)
        {
            var between90And110Percent = (int)Math.Ceiling(basicValue * _random.Next(90, 111) / 100);

            var tenPercent = (int)Math.Ceiling(basicValue / 10f);
            var adder = _random.Next(0, Math.Min(tenPercent, 6));

            return basicValue < 0
                ? between90And110Percent - adder
                : between90And110Percent + adder;
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
                    var targetingGameObject = Object.Instantiate(prefab, startPosition, Quaternion.identity);

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
                    var shapeGameObject = Object.Instantiate(prefab, spawnPosition, rotation);
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
