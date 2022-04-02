using System;
using System.Globalization;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Localization;
using FullPotential.Core.PlayerBehaviours;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class EffectHelper : IEffectHelper
    {
        // ReSharper disable once InconsistentNaming
        private static readonly System.Random _random = new System.Random();

        private readonly Localizer _localizer;
        private readonly ITypeRegistry _typeRegistry;
        private readonly IRpcHelper _rpcHelper;

        public EffectHelper(
            Localizer localizer,
            ITypeRegistry typeRegistry,
            IRpcHelper rpcHelper)
        {
            _localizer = localizer;
            _typeRegistry = typeRegistry;
            _rpcHelper = rpcHelper;
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

            if (itemUsed?.Effects != null && itemUsed.Effects.Any())
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

                if (source == null)
                {
                    Debug.LogWarning("Attack source not found. Did they sign out?");
                    return;
                }

                return;
            }

            ApplyDamage(source, itemUsed, target, position, target.GetComponent<IFighter>());
        }

        private void ApplyDamage(
            GameObject source,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position,
            IFighter targetFighter
        )
        {
            var sourceIsPlayer = source != null && source.CompareTag(Tags.Player);
            var sourcePlayerState = sourceIsPlayer ? source.GetComponent<PlayerState>() : null;

            //Even a small attack can still do damage
            var attackStrength = itemUsed?.Attributes.Strength ?? 1;
            var damageDealtBasic = attackStrength * 100f / (100 + targetFighter.GetDefenseValue());

            //Throw in some variation
            var multiplier = (float)_random.Next(90, 111) / 100;
            var adder = _random.Next(0, 6);
            var damageDealt = (int)Math.Ceiling(damageDealtBasic / multiplier) + adder;

            //TakeDamage call
            var sourceName = sourceIsPlayer
                ? sourcePlayerState.Username
                : (source != null ? source.name : null).OrIfNullOrWhitespace(_localizer.Translate("ui.alert.unknownattacker"));
            var sourceItemName = itemUsed?.Name ?? _localizer.Translate("ui.alert.attack.noitem");
            var sourceNetworkObject = source != null ? source.GetComponent<NetworkObject>() : null;
            var sourceClientId = source != null ? (ulong?)sourceNetworkObject.OwnerClientId : null;
            targetFighter.TakeDamage(damageDealt, sourceClientId, sourceName, sourceItemName);

            //Extras
            if (source == null)
            {
                Debug.LogWarning("Attack source not found. Did they sign out?");
                return;
            }

            if (itemUsed == null)
            {
                var targetRb = target.GetComponent<Rigidbody>();
                if (targetRb != null && position.HasValue)
                {
                    targetRb.AddForceAtPosition(source.transform.forward * 150, position.Value);
                }
            }

            if (sourceIsPlayer && position.HasValue && source != target)
            {
                sourcePlayerState.ShowDamageClientRpc(
                    position.Value,
                    damageDealt.ToString(CultureInfo.InvariantCulture),
                    _rpcHelper.ForPlayer(sourcePlayerState.OwnerClientId));
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
            //todo: attribute-based change value
            target.GetComponent<IFighter>().AddAttributeModifier(attributeEffect, attributes);
        }

        private void ApplyStatEffect(GameObject source, IStatEffect statEffect, ItemBase itemUsed, GameObject target, Vector3? position)
        {
            var targetFighter = target.GetComponent<IFighter>();

            switch (statEffect.Affect)
            {
                case Affect.PeriodicDecrease:
                case Affect.PeriodicIncrease:
                    targetFighter.ApplyPeriodicActionToStat(statEffect, itemUsed.Attributes);
                    return;

                case Affect.SingleDecrease:
                case Affect.SingleIncrease:

                    if (statEffect.Affect == Affect.SingleDecrease && statEffect.StatToAffect == AffectableStats.Health)
                    {
                        ApplyDamage(source, itemUsed, target, position, targetFighter);
                        return;
                    }

                    targetFighter.AlterValue(statEffect, itemUsed.Attributes);
                    return;

                case Affect.TemporaryMaxDecrease:
                case Affect.TemporaryMaxIncrease:
                    targetFighter.ApplyTemporaryMaxActionToStat(statEffect, itemUsed.Attributes);
                    return;

                default:
                    throw new NotImplementedException();
            }
        }

        private void ApplyMovementEffect(GameObject source, IMovementEffect movementEffect, Attributes attributes, GameObject target)
        {
            var rb = target.GetComponent<IFighter>().GetRigidBody();

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
                    var awayVector = rb.transform.position - source.GetComponent<IFighter>().GetRigidBody().transform.position;
                    rb.AddForce(awayVector * force, ForceMode.Acceleration);
                    return;

                case MovementDirection.TowardSource:
                    if (source == null)
                    {
                        Debug.LogWarning("Attack source not found. Did they sign out?");
                        return;
                    }
                    var towardVector = source.GetComponent<IFighter>().GetRigidBody().transform.position - rb.transform.position;
                    rb.AddForce(towardVector * force, ForceMode.Acceleration);
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
