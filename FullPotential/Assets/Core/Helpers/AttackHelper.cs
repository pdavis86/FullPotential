using FullPotential.Api.Behaviours;
using FullPotential.Core.Constants;
using FullPotential.Core.Registry.Base;
using Unity.Netcode;
using System;
using System.Globalization;
using FullPotential.Api.Enums;
using UnityEngine;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.EnemyBehaviours;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Extensions;

namespace FullPotential.Core.Helpers
{
    public static class AttackHelper
    {
        // ReSharper disable once InconsistentNaming
        private static readonly System.Random _random = new System.Random();

        public static void DealDamage(
            GameObject source,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position
        )
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("Player tried to deal damage instead of server");
                return;
            }

            var targetIsPlayer = target.CompareTag(Tags.Player);
            var targetIsEnemy = target.CompareTag(Tags.Enemy);

            if (!targetIsPlayer && !targetIsEnemy)
            {
                //Debug.Log($"You hit {target.name} which is not damageable");
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
                var enemyState = target.GetComponent<EnemyState>();
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

            var sourceClientId = source == null
                ? null
                : source.GetComponent<NetworkObject>()?.OwnerClientId;

            var sourceIsPlayer = source?.CompareTag(Tags.Player) ?? false;
            var sourcePlayerState = source?.GetComponent<PlayerState>();

            // ReSharper disable once StringLiteralTypo
            var sourceName = sourceIsPlayer
                ? sourcePlayerState.Username
                : source?.name.OrIfNullOrWhitespace(GameManager.Instance.Localizer.Translate("ui.alert.unknownattacker"));

            damageable.TakeDamage(damageDealt, sourceClientId, sourceName);

            if (source == null)
            {
                Debug.LogWarning("Attack source not found. Did they sign out?");
                return;
            }

            //Debug.Log($"Source '{source.name}' used '{itemUsed?.Name ?? "their fist"}' to attack target '{target.name}' for {damageDealt} damage");

            if (itemUsed == null)
            {
                var targetRb = target.GetComponent<Rigidbody>();
                if (targetRb != null && position.HasValue)
                {
                    targetRb.AddForceAtPosition(source.transform.forward * 150, position.Value);
                }
            }

            if (source.CompareTag(Tags.Player) && position.HasValue)
            {
                var offsetX = (float)_random.Next(-9, 10) / 100;
                var offsetY = (float)_random.Next(-9, 10) / 100;
                var offsetZ = (float)_random.Next(-9, 10) / 100;
                var adjustedPosition = position.Value + new Vector3(offsetX, offsetY, offsetZ);


                var clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { sourcePlayerState.OwnerClientId } } };
                sourcePlayerState.ShowDamageClientRpc(adjustedPosition, damageDealt.ToString(CultureInfo.InvariantCulture), clientRpcParams);
            }
        }

        public static void CheckIfOffTheMap(IDamageable damageable, float yValue)
        {
            if (damageable.AliveState != LivingEntityState.Dead && yValue < GameManager.Instance.SceneBehaviour.Attributes.LowestYValue)
            {
                damageable.HandleDeath(GameManager.Instance.Localizer.Translate("ui.alert.falldamage"));
            }
        }

    }
}
