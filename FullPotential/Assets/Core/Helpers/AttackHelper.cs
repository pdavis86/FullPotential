using FullPotential.Assets.Core.Constants;
using FullPotential.Assets.Core.Registry.Base;
using MLAPI;
using MLAPI.Messaging;
using System;
using System.Globalization;
using UnityEngine;

namespace FullPotential.Assets.Core.Helpers
{
    public static class AttackHelper
    {
        // ReSharper disable once InconsistentNaming
        private static readonly System.Random _random = new System.Random();

        public static void DealDamage(
            GameObject source,
            ItemBase sourceItem,
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
                //Debug.Log("You hit something not damageable");
                return;
            }

            var attackStrength = sourceItem.Attributes.Strength;

            int defenceStrength;
            if (targetIsPlayer)
            {
                var otherPlayerState = target.GetComponent<PlayerState>();
                defenceStrength = otherPlayerState.Inventory.GetDefenseValue();
            }
            else
            {
                //todo: calc defence
                defenceStrength = 1;
            }

            const int swingValue = 20;
            var numerator = 100 + _random.Next(0, swingValue);
            var denominator = 100 + _random.Next(swingValue * -1, swingValue);
            var damageDealt = Math.Round(attackStrength * ((double)numerator / (denominator + defenceStrength)), 0);

            Debug.Log($"Source '{source.name}' used '{sourceItem.Name}' to attack target '{target.name}' for {damageDealt} damage");

            if (source.CompareTag(Tags.Player) && position.HasValue)
            {
                var offsetX = (float)_random.Next(-9, 10) / 100;
                var offsetY = (float)_random.Next(-9, 10) / 100;
                var offsetZ = (float)_random.Next(-9, 10) / 100;
                var adjustedPosition = position.Value + new Vector3(offsetX, offsetY, offsetZ);

                var playerState = source.GetComponent<PlayerState>();
                var clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { playerState.OwnerClientId } } };
                playerState.ShowDamageClientRpc(adjustedPosition, damageDealt.ToString(CultureInfo.InvariantCulture), clientRpcParams);
            }
        }

    }
}
