using System;
using System.Globalization;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Localization;
using FullPotential.Core.PlayerBehaviours;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class AttackHelper : IAttackHelper
    {
        public const int MeleeRangeLimit = 8;

        // ReSharper disable once InconsistentNaming
        private static readonly System.Random _random = new System.Random();

        private readonly Localizer _localizer;

        public AttackHelper(Localizer localizer)
        {
            _localizer = localizer;
        }

        public void DealDamage(
            GameObject source,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position
        )
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Player tried to deal damage instead of server");
                return;
            }

            var targetIsPlayer = target.CompareTag(Tags.Player);
            var targetIsEnemy = target.CompareTag(Tags.Enemy);

            if (!targetIsPlayer && !targetIsEnemy)
            {
                return;
            }

            IDamageable damageable;
            int defenceStrength;
            if (targetIsPlayer)
            {
                var otherPlayerState = target.GetComponent<PlayerState>();
                damageable = otherPlayerState;
                defenceStrength = otherPlayerState.Inventory.GetDefenseValue();
            }
            else
            {
                var enemyState = target.GetComponent<IEnemyStateBehaviour>();
                damageable = enemyState;
                defenceStrength = enemyState.GetDefenseValue();
            }

            //Even a small attack can still do damage
            var attackStrength = itemUsed?.Attributes.Strength ?? 1;
            var damageDealtBasic = attackStrength * 100f / (100 + defenceStrength);

            //Throw in some variation
            var multiplier = (float)_random.Next(90, 111) / 100;
            var adder = _random.Next(0, 6);
            var damageDealt = (int)Math.Ceiling(damageDealtBasic / multiplier) + adder;

            var sourceNetworkObject = source != null ? source.GetComponent<NetworkObject>() : null;
            var sourceClientId = source != null ? (ulong?)sourceNetworkObject.OwnerClientId : null;

            var sourceIsPlayer = source != null && source.CompareTag(Tags.Player);
            var sourcePlayerState = sourceIsPlayer ? source.GetComponent<PlayerState>() : null;

            var sourceName = sourceIsPlayer
                ? sourcePlayerState.Username
                : (source != null ? source.name : null).OrIfNullOrWhitespace(_localizer.Translate("ui.alert.unknownattacker"));

            var sourceItemName = itemUsed?.Name ?? _localizer.Translate("ui.alert.attack.noitem");

            damageable.TakeDamage(damageDealt, sourceClientId, sourceName, sourceItemName);

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
                var offsetX = (float)_random.Next(-9, 10) / 100;
                var offsetY = (float)_random.Next(-9, 10) / 100;
                var offsetZ = (float)_random.Next(-9, 10) / 100;
                var adjustedPosition = position.Value + new Vector3(offsetX, offsetY, offsetZ);

                var clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { sourcePlayerState.OwnerClientId } } };
                sourcePlayerState.ShowDamageClientRpc(adjustedPosition, damageDealt.ToString(CultureInfo.InvariantCulture), clientRpcParams);
            }
        }

        public void CheckIfOffTheMap(IDamageable damageable, float yValue)
        {
            if (damageable.AliveState != LivingEntityState.Dead && yValue < GameManager.Instance.GetSceneBehaviour().Attributes.LowestYValue)
            {
                damageable.HandleDeath(_localizer.Translate("ui.alert.falldamage"), null);
            }
        }

        public string GetDeathMessage(bool isOwner, string victimName, string killerName, string itemName)
        {
            if (itemName.IsNullOrWhiteSpace())
            {
                return isOwner
                    ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledby"), killerName)
                    : string.Format(_localizer.Translate("ui.alert.attack.victimkilledby"), victimName, killerName);
            }

            return isOwner
                ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledbyusing"), killerName, itemName)
                : string.Format(_localizer.Translate("ui.alert.attack.victimkilledbyusing"), victimName, killerName, itemName);
        }

    }
}
