using System.Linq;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class EffectService : IEffectService
    {
        private readonly ITypeRegistry _typeRegistry;

        public EffectService(
            ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
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

                targetFighter.TakeDamage(sourceFighter, itemUsed, position);
                return;
            }

            foreach (var effect in itemUsed.Effects)
            {
                ApplyEffect(sourceFighter, effect, itemUsed, target, position);

                if (sourceFighter != null && effect is IHasSideEffect withSideEffect)
                {
                    var sideEffect = _typeRegistry.GetEffect(withSideEffect.SideEffectType);
                    ApplyEffect(null, sideEffect, itemUsed, sourceFighter.GameObject, position);
                }
            }
        }

        private void ApplyEffect(IFighter sourceFighter, IEffect effect, ItemBase itemUsed, GameObject targetGameObject, Vector3? position)
        {
            var targetFighter = targetGameObject.GetComponent<IFighter>();

            //Debug.Log($"Applying {effect.TypeName} to {targetFighter.FighterName}");

            switch (effect)
            {
                case IStatEffect statEffect:
                    ApplyStatEffect(targetFighter, statEffect, itemUsed, sourceFighter, position);
                    return;

                case IMovementEffect movementEffect:
                    ApplyMovementEffect(targetGameObject, movementEffect, itemUsed.Attributes, sourceFighter);
                    return;

                case IAttributeEffect attributeEffect:
                    ApplyAttributeEffect(targetFighter, attributeEffect, itemUsed.Attributes);
                    return;

                case IElement elementalEffect:
                    ApplyElementalEffect(targetFighter, elementalEffect, itemUsed.Attributes);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for effect {effect}");
                    return;
            }
        }

        private void ApplyAttributeEffect(IFighter targetFighter, IAttributeEffect attributeEffect, Attributes attributes)
        {
            targetFighter.AddAttributeModifier(attributeEffect, attributes);
        }

        private void ApplyStatEffect(IFighter targetFighter, IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            switch (statEffect.Affect)
            {
                case Affect.PeriodicDecrease:
                case Affect.PeriodicIncrease:
                    targetFighter.ApplyPeriodicActionToStat(statEffect, itemUsed.Attributes);
                    return;

                case Affect.SingleDecrease:
                case Affect.SingleIncrease:
                    targetFighter.ApplyStatValueChange(statEffect, itemUsed, sourceFighter, position);
                    return;

                case Affect.TemporaryMaxDecrease:
                case Affect.TemporaryMaxIncrease:
                    targetFighter.ApplyTemporaryMaxActionToStat(statEffect, itemUsed.Attributes);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for affect {statEffect.Affect}");
                    return;
            }
        }

        private void ApplyMovementEffect(GameObject targetGameObject, IMovementEffect movementEffect, Attributes attributes, IFighter sourceFighter)
        {
            var targetRigidBody = targetGameObject.GetComponent<Rigidbody>();

            if (targetRigidBody == null)
            {
                Debug.LogWarning($"Cannot move target '{targetGameObject.name}' as it does not have a RigidBody");
                return;
            }

            var force = AttributeCalculator.GetForceValue(attributes);

            switch (movementEffect.Direction)
            {
                case MovementDirection.AwayFromSource:
                case MovementDirection.TowardSource:

                    if (sourceFighter == null)
                    {
                        Debug.LogWarning("Attack source not found. Did they sign out?");
                        return;
                    }

                    var sourcePosition = sourceFighter.RigidBody.transform.position;
                    var targetPosition = targetRigidBody.transform.position;

                    var vector = movementEffect.Direction == MovementDirection.AwayFromSource
                        ? targetPosition - sourcePosition
                        : sourcePosition - targetPosition;

                    targetRigidBody.AddForce(vector * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Backwards:
                    targetRigidBody.AddForce(-sourceFighter.Transform.forward * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Forwards:
                    targetRigidBody.AddForce(sourceFighter.Transform.forward * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Down:
                    targetRigidBody.AddForce(-sourceFighter.Transform.up * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Up:
                    targetRigidBody.AddForce(sourceFighter.Transform.up * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Left:
                    targetRigidBody.AddForce(-sourceFighter.Transform.right * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Right:
                    targetRigidBody.AddForce(sourceFighter.Transform.right * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.MaintainDistance:
                    sourceFighter.BeginMaintainDistanceOn(targetGameObject);
                    return;

                default:
                    Debug.LogError($"Not implemented handling for movement direction {movementEffect.Direction}");
                    return;
            }
        }

        private void ApplyElementalEffect(IFighter targetFighter, IEffect elementalEffect, Attributes attributes)
        {
            targetFighter.ApplyElementalEffect(elementalEffect, attributes);
        }

    }
}
