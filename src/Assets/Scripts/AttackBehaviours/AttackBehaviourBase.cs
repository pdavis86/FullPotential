using Assets.Core.Constants;
using Assets.Core.Registry.Base;
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

    private PlayerClientSide _playerClientSide;
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

        if (_playerClientSide == null)
        {
            _playerClientSide = _sourcePlayer.GetComponent<PlayerClientSide>();

            var playerState = _sourcePlayer.GetComponent<PlayerState>();

            _clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { playerState.ClientId.Value }
                }
            };
        }

        var targetIsPlayer = target.CompareTag(Tags.Player);
        var targetIsEnemy = target.CompareTag(Tags.Enemy);

        if (!targetIsPlayer && !targetIsEnemy)
        {
            //Debug.Log("You hit something not damageable");
            return;
        }

        //todo: implement AttackBehaviourBase crit? if so, what is it?
        //todo: implement AttackBehaviourBase half-damage for duel-weilding
        //todo: show when elemental effects are in use - GameManager.Instance.Prefabs.Combat.ElementalText

        //todo: calc defence
        var defenceStrength = 30;

        var numerator = 100 + _random.Next(0, 10);
        var denominator = 100 + _random.Next(-10, 10);
        var damageDealt = Math.Round(sourceItem.Attributes.Strength * ((double)numerator / (denominator + defenceStrength)), 0);

        Debug.Log($"Player '{_sourcePlayer.name}' used '{sourceItem.Name}' to attack target '{target.name}' for {damageDealt} damage");

        //todo: For dealing damage, look at https://docs.unity3d.com/2018.1/Documentation/ScriptReference/Networking.SyncEventAttribute.html

        //todo: check Luck then apply lingering

        _playerClientSide.ShowDamageClientRpc(position, damageDealt.ToString(CultureInfo.InvariantCulture), _clientRpcParams);

        //todo: give source experience
    }

}
