using System;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using FullPotential.Api.Unity.Constants;
using FullPotential.Core.Localization;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class EffectService : IEffectService
    {
        private readonly Localizer _localizer;
        private readonly ITypeRegistry _typeRegistry;
        private readonly IRpcService _rpcService;

        public EffectService(
            Localizer localizer,
            ITypeRegistry typeRegistry,
            IRpcService rpcService)
        {
            _localizer = localizer;
            _typeRegistry = typeRegistry;
            _rpcService = rpcService;
        }

        public void ApplyEffects(
            GameObject source,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position
        )
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Player tried to apply effects instead of server");
                return;
            }

            if (!target.CompareTag(Tags.Player) && !target.CompareTag(Tags.Enemy))
            {
                Debug.LogWarning("Target is neither a player nor enemy. Target was: " + target);
                return;
            }

            var itemHasEffects = itemUsed?.Effects != null && itemUsed.Effects.Any();

            if (!itemHasEffects)
            {
                var targetFighter = target.GetComponent<IDamageable>();
                if (targetFighter != null)
                {
                    targetFighter.TakeDamage(source, itemUsed, position);
                }
                else
                {
                    Debug.LogWarning("Target was not an IDamageable");
                }
            }
            else
            {
                foreach (var effect in itemUsed.Effects)
                {
                    ApplyEffect(source, effect, itemUsed, target, position);

                    if (source != null && effect is IHasSideEffect withSideEffect)
                    {
                        var sideEffect = _typeRegistry.GetEffect(withSideEffect.SideEffectType);
                        ApplyEffect(null, sideEffect, itemUsed, source, position);
                    }
                }
            }

            if (source == null)
            {
                Debug.LogWarning("Attack source not found. Did they sign out?");
            }
        }

        private void ApplyEffect(GameObject source, IEffect effect, ItemBase itemUsed, GameObject target, Vector3? position)
        {
            //var sourceFighter = source != null ? source.GetComponent<IFighter>() : null;
            //var targetFighter = target.GetComponent<IFighter>();

            switch (effect)
            {
                case IAttributeEffect attributeEffect:
                    ApplyAttributeEffect(attributeEffect, itemUsed.Attributes, target);
                    return;

                case IStatEffect statEffect:
                    ApplyStatEffect(source, statEffect, itemUsed, target, position);
                    return;

                case IMovementEffect movementEffect:
                    ApplyMovementEffect(source, movementEffect, itemUsed.Attributes, target);
                    return;

                case IElement elementalEffect:
                    ApplyElementalEffect(elementalEffect, itemUsed.Attributes, target);
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        private void ApplyAttributeEffect(IAttributeEffect attributeEffect, Attributes attributes, GameObject target)
        {
            //todo: target.GetComponent<IFighter>().AddAttributeModifier(attributeEffect, attributes);
        }

        private void ApplyStatEffect(GameObject source, IStatEffect statEffect, ItemBase itemUsed, GameObject target, Vector3? position)
        {
            var targetFighter = target.GetComponent<IFighter>();

            switch (statEffect.Affect)
            {
                case Affect.PeriodicDecrease:
                case Affect.PeriodicIncrease:
                    //todo: targetFighter.ApplyPeriodicActionToStat(statEffect, itemUsed.Attributes);
                    return;

                case Affect.SingleDecrease:
                case Affect.SingleIncrease:

                    if (statEffect.Affect == Affect.SingleDecrease && statEffect.StatToAffect == AffectableStats.Health)
                    {
                        targetFighter.TakeDamage(source, itemUsed, position);
                        return;
                    }

                    //todo: targetFighter.AlterValue(statEffect, itemUsed.Attributes);
                    return;

                case Affect.TemporaryMaxDecrease:
                case Affect.TemporaryMaxIncrease:
                    //todo: targetFighter.ApplyTemporaryMaxActionToStat(statEffect, itemUsed.Attributes);
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        private void ApplyMovementEffect(GameObject source, IMovementEffect movementEffect, Attributes attributes, GameObject target)
        {
            var targetFighter = target.GetComponent<IFighter>();
            var targetRigidBody = targetFighter.RigidBody;

            //todo: handle periodic

            //todo: attribute-based force value
            var force = 100f;

            switch (movementEffect.Direction)
            {
                case MovementDirection.AwayFromSource:
                    if (source == null)
                    {
                        Debug.LogWarning("Attack source not found. Did they sign out?");
                        return;
                    }
                    var awayVector = targetRigidBody.transform.position - source.GetComponent<IFighter>().RigidBody.transform.position;
                    targetRigidBody.AddForce(awayVector * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.TowardSource:
                    if (source == null)
                    {
                        Debug.LogWarning("Attack source not found. Did they sign out?");
                        return;
                    }
                    var towardVector = source.GetComponent<IFighter>().RigidBody.transform.position - targetRigidBody.transform.position;
                    targetRigidBody.AddForce(towardVector * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.Backwards:
                case MovementDirection.Forwards:

                case MovementDirection.Down:
                case MovementDirection.Up:

                case MovementDirection.Left:
                case MovementDirection.Right:

                case MovementDirection.MaintainDistance:

                default:
                    throw new NotImplementedException();
            }
        }

        private void ApplyElementalEffect(IEffect elementalEffect, Attributes attributes, GameObject target)
        {
            throw new NotImplementedException();
        }

    }
}
