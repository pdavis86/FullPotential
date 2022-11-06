using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.SpellsAndGadgets;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class EffectService : IEffectService
    {
        private readonly ITypeRegistry _typeRegistry;
        private readonly IValueCalculator _valueCalculator;

        public EffectService(
            ITypeRegistry typeRegistry,
            IValueCalculator valueCalculator)
        {
            _typeRegistry = typeRegistry;
            _valueCalculator = valueCalculator;
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

            var itemHasEffects = itemUsed?.Effects != null && itemUsed.Effects.Any();

            if (!itemHasEffects)
            {
                var targetFighter = target.GetComponent<IFighter>();

                if (targetFighter == null)
                {
                    Debug.LogWarning("Target is not an IFighter. Target was: " + target);
                    return;
                }

                //Debug.Log($"Applying just damage (no effects) to {targetFighter.FighterName}");

                targetFighter.TakeDamageFromFighter(sourceFighter, itemUsed, position);
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

                ApplyEffect(sourceFighter, effect, itemUsed, target, position);

                if (sourceFighter != null && effect is IHasSideEffect withSideEffect)
                {
                    var sideEffect = _typeRegistry.GetEffect(withSideEffect.SideEffectType);
                    ApplyEffect(null, sideEffect, itemUsed, sourceFighter.GameObject, position);
                }
            }
        }

        private bool IsEffectAllowed(ItemBase itemUsed, GameObject target, IEffect effect)
        {
            if (itemUsed is SpellOrGadgetItemBase sog
                && sog.Targeting.IsContinuous)
            {
                if (effect is IMovementEffect movementEffect
                    && movementEffect.Direction == MovementDirection.MaintainDistance)
                {
                    if (target.GetComponent<MaintainDistance>() != null)
                    {
                        //Debug.Log("Continuing to hold target");
                        return false;
                    }

                    if (target.GetComponent<PlayerBehaviours.PlayerState>() != null)
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
                    ApplyElementalEffect(targetFighter, elementalEffect, itemUsed, sourceFighter, position);
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
                    targetFighter.ApplyStatValueChange(statEffect, itemUsed, sourceFighter, position);
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

        private void ApplyElementalEffect(IFighter targetFighter, IEffect elementalEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            targetFighter.ApplyElementalEffect(elementalEffect, itemUsed, sourceFighter, position);
        }

        private void ApplyMaintainDistance(ItemBase itemUsed, GameObject targetGameObject, IFighter sourceFighter)
        {
            if (itemUsed is not SpellOrGadgetItemBase sog || !sog.Targeting.IsContinuous)
            {
                Debug.LogWarning("MaintainDistance has been incorrectly applied to an item");
                return;
            }

            var comp = targetGameObject.AddComponent<MaintainDistance>();
            comp.SourceFighter = sourceFighter;
            comp.Distance = (targetGameObject.transform.position - sourceFighter.Transform.position).magnitude;
            comp.ItemUsed = sog;
        }

        private void ApplyMovementEffect(GameObject targetGameObject, IMovementEffect movementEffect, ItemBase itemUsed, IFighter sourceFighter)
        {
            var targetRigidBody = targetGameObject.GetComponent<Rigidbody>();

            if (targetRigidBody == null)
            {
                Debug.LogWarning($"Cannot move target '{targetGameObject.name}' as it does not have a RigidBody");
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

            var adjustForGravity = movementEffect.Direction is MovementDirection.Up or MovementDirection.Down;
            var force = _valueCalculator.GetMovementForceValue(itemUsed, adjustForGravity);

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

                        targetRigidBody.AddForce(forwardsBackwardsDirection.normalized * force, ForceMode.Acceleration);
                        return;
                    }
                    else
                    {
                        var leftRightDirection = movementEffect.Direction == MovementDirection.LeftFromSource
                            ? -sourceFighter.RigidBody.transform.right
                            : sourceFighter.RigidBody.transform.right;

                        targetRigidBody.AddForce(leftRightDirection * force, ForceMode.Acceleration);
                        return;
                    }

                case MovementDirection.Backwards:
                    targetRigidBody.AddForce(-targetGameObject.transform.forward * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Forwards:
                    targetRigidBody.AddForce(targetGameObject.transform.forward * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Down:
                    targetRigidBody.AddForce(-targetGameObject.transform.up * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Up:
                    targetRigidBody.AddForce(targetGameObject.transform.up * force, ForceMode.Acceleration);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for movement direction {movementEffect.Direction}");
                    return;
            }
        }

    }
}
