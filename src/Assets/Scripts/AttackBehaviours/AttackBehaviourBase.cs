using Assets.Core.Constants;
using Assets.Core.Registry.Base;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable NotAccessedField.Global

public abstract class AttackBehaviourBase : NetworkBehaviour
{
    protected GameObject SourcePlayer;

    // ReSharper disable once InconsistentNaming
    private static readonly System.Random _random = new System.Random();

    internal void DealDamage(ItemBase sourceItem, GameObject source, GameObject target, Vector3 position)
    {
        if (!isServer)
        {
            Debug.LogError("Player tried to deal damage instead of server");
            return;
        }

        //todo: implement AttackBehaviourBase crit? if so, what is it?
        //todo: implement AttackBehaviourBase half-damage for duel-weilding

        //todo: give source experience
        Debug.Log($"Source {source.name} would gain experience");

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

        Debug.Log($"Source '{sourceItem.Name}' attacked target '{target.name}' for {damageDealt} damage");

        //todo: For dealing damage, look at https://docs.unity3d.com/2018.1/Documentation/ScriptReference/Networking.SyncEventAttribute.html



        //todo: check Luck then apply lingering


        var networkIdentity = SourcePlayer.GetComponent<NetworkIdentity>();
        var controller = SourcePlayer.GetComponent<PlayerController>();
        controller.TargetRpcShowDamage(networkIdentity.connectionToClient, position, damageDealt.ToString(CultureInfo.InvariantCulture));
    }

}
