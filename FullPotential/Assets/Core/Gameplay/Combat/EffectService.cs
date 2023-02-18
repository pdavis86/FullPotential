using System;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class EffectService : IEffectService
    {
        public static readonly System.Random Random = new System.Random();

        private readonly ITypeRegistry _typeRegistry;
        private readonly IRpcService _rpcService;
        private readonly Punch _punchEffect;

        public EffectService(
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
            Vector3? position
        )
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("ApplyEffects was not called on the server");
                return;
            }

            if (itemUsed == null)
            {
                itemUsed = new Loot { Attributes = new Attributes { Strength = sourceFighter.GetAttributeValue(AffectableAttribute.Strength) } };
                ApplyEffect(sourceFighter, _punchEffect, itemUsed, target, position);
            }

            var itemHasEffects = itemUsed.Effects != null && itemUsed.Effects.Any();
            var itemIsWeapon = itemUsed is Weapon;

            if (!itemHasEffects || itemIsWeapon)
            {
                var targetFighter = target.GetComponent<IFighter>();

                if (targetFighter == null)
                {
                    //Debug.LogWarning("Target is not an IFighter. Target was: " + target);

                    //todo: zzz v0.6 - SpawnBulletHole: move this, feels like the wrong place
                    var registryType = (IGearWeapon)itemUsed.RegistryType;
                    var isRanged = registryType?.Category == WeaponCategory.Ranged;
                    if (isRanged)
                    {
                        SpawnBulletHole(target, position);
                    }

                    return;
                }

                //Debug.Log($"Applying just damage (no effects) to {targetFighter.FighterName}");

                var damageDealt = GetDamageValueFromAttack(itemUsed, targetFighter.GetDefenseValue()) * -1;

                var sourceFighterCriticalHitChance = sourceFighter.GetCriticalHitChance();
                var critTestValue = Random.Next(0, 101);
                var isCritical = critTestValue <= sourceFighterCriticalHitChance;

                if (isCritical)
                {
                    //Debug.Log($"CRITICAL! Chance:{sourceFighterCriticalHitChance}, test:{critTestValue}");

                    damageDealt *= 2;
                }

                targetFighter.TakeDamageFromFighter(sourceFighter, itemUsed, position, damageDealt, isCritical);

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

                //todo: scale down by number of effects
                ApplyEffect(sourceFighter, effect, itemUsed, target, position);

                if (sourceFighter != null && effect is IHasSideEffect withSideEffect)
                {
                    var sideEffect = _typeRegistry.GetEffect(withSideEffect.SideEffectType);
                    ApplyEffect(null, sideEffect, itemUsed, sourceFighter.GameObject, position);
                }
            }
        }

        private void SpawnBulletHole(
            GameObject target,
            Vector3? position)
        {
            //todo: zzz v0.6 - SpawnBulletHole only works for box colliders that line up with the X and Z alias

            if (!position.HasValue)
            {
                return;
            }

            var targetCollider = target.GetComponent<BoxCollider>();

            if (targetCollider == null)
            {
                return;
            }

            var vertices = GameObjectHelper.GetBoxColliderVertices(targetCollider);

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
            bulletHole.GetComponent<NetworkObject>().Spawn();
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

        private void ApplyEffect(IFighter sourceFighter, IEffect effect, ItemBase itemUsed, GameObject targetGameObject, Vector3? position)
        {
            if (effect is IMovementEffect movementEffect)
            {
                ApplyMovementEffect(targetGameObject, movementEffect, itemUsed, sourceFighter);
                return;
            }

            var targetFighter = targetGameObject.GetComponent<IFighter>();

            if (targetFighter == null)
            {
                Debug.LogWarning($"Not applying {effect.TypeName} to {targetGameObject.name} because they are not an IFighter");
                return;
            }

            //Debug.Log($"Applying {effect.TypeName} to {targetFighter.FighterName}");

            switch (effect)
            {
                case IStatEffect statEffect:
                    ApplyStatEffect(targetFighter, statEffect, itemUsed, sourceFighter, position);
                    return;

                case IAttributeEffect attributeEffect:
                    ApplyAttributeEffect(targetFighter, attributeEffect, itemUsed);
                    return;

                case IElement elementalEffect:
                    var damageDealt = GetDamageValueFromAttack(itemUsed, targetFighter.GetDefenseValue()) * -1;
                    ApplyElementalEffect(targetFighter, elementalEffect, itemUsed, sourceFighter, damageDealt, position);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for effect {effect}");
                    return;
            }
        }

        private void ApplyStatEffect(IFighter targetFighter, IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            switch (statEffect.Affect)
            {
                case Affect.PeriodicDecrease:
                case Affect.PeriodicIncrease:
                    targetFighter.ApplyPeriodicActionToStat(statEffect, itemUsed, sourceFighter);
                    return;

                case Affect.SingleDecrease:
                case Affect.SingleIncrease:

                    var change = itemUsed.GetStatChange(statEffect);

                    if (statEffect.StatToAffect == AffectableStat.Health && statEffect.Affect == Affect.SingleDecrease)
                    {
                        change = GetDamageValueFromAttack(itemUsed, targetFighter.GetDefenseValue()) * -1;
                    }

                    targetFighter.ApplyStatValueChange(statEffect, itemUsed, sourceFighter, change, position);
                    return;

                case Affect.TemporaryMaxDecrease:
                case Affect.TemporaryMaxIncrease:
                    targetFighter.ApplyTemporaryMaxActionToStat(statEffect, itemUsed, sourceFighter, position);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for affect {statEffect.Affect}");
                    return;
            }
        }

        private void ApplyAttributeEffect(IFighter targetFighter, IAttributeEffect attributeEffect, ItemBase itemUsed)
        {
            targetFighter.AddAttributeModifier(attributeEffect, itemUsed);
        }

        private void ApplyElementalEffect(IFighter targetFighter, IEffect elementalEffect, ItemBase itemUsed, IFighter sourceFighter, int change, Vector3? position)
        {
            targetFighter.ApplyElementalEffect(elementalEffect, itemUsed, sourceFighter, change, position);
        }

        private void ApplyMaintainDistance(ItemBase itemUsed, GameObject targetGameObject, IFighter sourceFighter)
        {
            if (itemUsed is not Consumer consumer || !consumer.Targeting.IsContinuous)
            {
                Debug.LogWarning("MaintainDistance has been incorrectly applied to an item");
                return;
            }

            //todo: handle networking

            var comp = targetGameObject.AddComponent<MaintainDistance>();
            comp.SourceFighter = sourceFighter;
            comp.Distance = (targetGameObject.transform.position - sourceFighter.Transform.position).magnitude;
            comp.Consumer = consumer;
        }

        private void ApplyMovementEffect(GameObject targetGameObject, IMovementEffect movementEffect, ItemBase itemUsed, IFighter sourceFighter)
        {
            var targetRigidBody = targetGameObject.GetComponent<Rigidbody>();

            if (targetRigidBody == null)
            {
                Debug.LogWarning($"Cannot move target '{targetGameObject.name}' as it does not have a RigidBody");
                return;
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

            var targetLivingEntity = targetGameObject.GetComponent<LivingEntityBase>();

            if (targetLivingEntity != null)
            {
                targetLivingEntity.SetLastMover(sourceFighter);
            }

            var adjustForGravity = movementEffect.Direction is MovementDirection.Up or MovementDirection.Down;
            var force = itemUsed.GetMovementForceValue(adjustForGravity);

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


        private int AddVariationToValue(double basicValue)
        {
            var multiplier = (double)Random.Next(90, 111) / 100;
            var adder = Random.Next(0, 6);
            return (int)Math.Ceiling(basicValue / multiplier) + adder;
        }

        public int GetDamageValueFromAttack(ItemBase itemUsed, int targetDefense, bool addVariation = true)
        {
            var weapon = itemUsed as Weapon;
            var weaponCategory = (weapon?.RegistryType as IGearWeapon)?.Category;

            float attackStrength = itemUsed?.Attributes.Strength ?? 1;
            var defenceRatio = 100f / (100 + targetDefense);
            
            if (weaponCategory == WeaponCategory.Ranged)
            {
                attackStrength /= weapon.GetBulletsPerSecond();
            }

            //Even a small attack can still do damage
            var damageDealtBasic = Math.Ceiling(attackStrength * defenceRatio / ItemBase.StrengthDivisor);

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

            return AddVariationToValue(damageDealtBasic);
        }

    }
}
