using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items;
using FullPotential.Api.Logging;
using FullPotential.Api.Networking;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;

using Unity.Netcode;

using UnityEngine;

namespace FullPotential.Core.Gameplay.Combat
{
    public class MovementEffectHandler : ICombatEffectHandler
    {
        private readonly IAuditor _logger;
        private readonly IRpcService _rpcService;

        public MovementEffectHandler(IAuditorFactory auditorFactory, IRpcService rpcService)
        {
            _logger = auditorFactory.Create(this);
            _rpcService = rpcService;
        }

        public bool CanHandle(IEffectType effect) => effect is IMovementEffectType;

        public void Apply(CombatEffectContext context, IEffectType effect)
        {
            ApplyMovementEffect(context.SourceFighter, context.ItemUsed, (IMovementEffectType)effect, context.Target);
        }

        private void ApplyMaintainDistance(FighterBase sourceFighter, ICombatItem itemUsed, GameObject targetGameObject)
        {
            if (itemUsed is not IConsumer consumer || !consumer.TargetingType.IsContinuous)
            {
                _logger.Warn("MaintainDistance has been incorrectly applied to an item");
                return;
            }

            var comp = targetGameObject.AddComponent<MaintainDistance>();
            comp.SourceFighter = sourceFighter;
            comp.Distance = (targetGameObject.transform.position - sourceFighter.Transform.position).magnitude;
            comp.Consumer = consumer;
        }

        private void ApplyMovementEffect(FighterBase sourceFighter, ICombatItem itemUsed, IMovementEffectType movementEffect, GameObject targetGameObject)
        {
            if (!targetGameObject.TryGetComponent<Rigidbody>(out var targetRigidBody))
            {
                _logger.Warn("Cannot move target '{0}' as it does not have a RigidBody", targetGameObject.name);
                return;
            }

            if (targetGameObject.TryGetComponent<LivingEntityBase>(out var targetLivingEntity))
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
                _logger.Warn("Cannot apply a movement effect to target '{0}' as it does not have a NetworkObject component", targetGameObject.name);
                return;
            }

            if (!targetGameObject.TryGetComponent<IMoveable>(out var targetMoveable))
            {
                _logger.Warn("Cannot apply a movement effect to target '{0}' as it has no components that implement {1}", targetGameObject.name, nameof(IMoveable));
                return;
            }

            var adjustForGravity = movementEffect.Direction is MovementDirection.Up or MovementDirection.Down;
            var strength = itemUsed?.Attributes.GetValueOrZero(Strength.Id) ?? sourceFighter.GetAdjustedAttributeValue(Strength.Id);
            var rawForce = MathsHelper.GetHighInHighOutInRange(strength, 100, 300);
            var force = adjustForGravity ? rawForce * 2f : rawForce;

            Vector3 forceToApply;
            switch (movementEffect.Direction)
            {
                case MovementDirection.AwayFromSource:
                case MovementDirection.TowardSource:
                case MovementDirection.LeftFromSource:
                case MovementDirection.RightFromSource:
                    if (sourceFighter == null)
                    {
                        _logger.Warn("Attack source not found. Did they sign out?");
                        return;
                    }

                    if (movementEffect.Direction is MovementDirection.AwayFromSource or MovementDirection.TowardSource)
                    {
                        var sourcePosition = sourceFighter.RigidBody.transform.position;
                        var targetPosition = targetRigidBody.transform.position;
                        var forwardsBackwardsDirection = movementEffect.Direction == MovementDirection.AwayFromSource
                            ? targetPosition - sourcePosition
                            : sourcePosition - targetPosition;
                        if (forwardsBackwardsDirection == Vector3.zero)
                        {
                            forwardsBackwardsDirection = -sourceFighter.RigidBody.transform.forward;
                        }

                        _logger.Debug("Applying {0} force to {1}", force, targetGameObject.name);
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
                    _logger.Error("Not implemented handling for movement direction {0}", movementEffect.Direction);
                    return;
            }

            const ForceMode forceMode = ForceMode.Force;
            targetRigidBody.AddForce(forceToApply, forceMode);
            var nearbyClients = _rpcService.ForNearbyPlayersExcept(targetGameObject.transform.position, 0);
            targetMoveable.ApplyMovementForceClientRpc(forceToApply, forceMode, nearbyClients);
        }
    }
}
