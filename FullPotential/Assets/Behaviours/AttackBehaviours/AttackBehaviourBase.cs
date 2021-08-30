using FullPotential.Assets.Core.Constants;
using FullPotential.Assets.Core.Registry.Base;
using MLAPI;
using MLAPI.Messaging;
using System;
using System.Globalization;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable InconsistentNaming

public abstract class AttackBehaviourBase : NetworkBehaviour
{
    protected GameObject _sourcePlayer;

    private PlayerState _playerState;
    private ClientRpcParams _clientRpcParams;

    // ReSharper disable once InconsistentNaming
    private static readonly System.Random _random = new System.Random();

    protected void DealDamage(ItemBase sourceItem, GameObject source, GameObject target, Vector3 position)
    {
        if (!IsServer)
        {
            Debug.LogError("Player tried to deal damage instead of server");
            return;
        }

        if (_playerState == null)
        {
            _playerState = _sourcePlayer.GetComponent<PlayerState>();
            _clientRpcParams.Send.TargetClientIds = new[] { _playerState.OwnerClientId };
        }

        var targetIsPlayer = target.CompareTag(Tags.Player);
        var targetIsEnemy = target.CompareTag(Tags.Enemy);

        if (!targetIsPlayer && !targetIsEnemy)
        {
            //Debug.Log("You hit something not damageable");
            return;
        }

        //todo: calc defence
        var defenceStrength = 30;

        var numerator = 100 + _random.Next(0, 10);
        var denominator = 100 + _random.Next(-10, 10);
        var damageDealt = Math.Round(sourceItem.Attributes.Strength * ((double)numerator / (denominator + defenceStrength)), 0);

        //Debug.Log($"Player '{_sourcePlayer.name}' used '{sourceItem.Name}' to attack target '{target.name}' for {damageDealt} damage");

        _playerState.ShowDamageClientRpc(position, damageDealt.ToString(CultureInfo.InvariantCulture), _clientRpcParams);
    }

}
