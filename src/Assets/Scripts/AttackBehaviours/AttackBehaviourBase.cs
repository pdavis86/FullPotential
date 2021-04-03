﻿using Assets.Core.Crafting;
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
    public GameObject SourcePlayer;

    // ReSharper disable once InconsistentNaming
    private static readonly System.Random _random = new System.Random();

    internal void DealDamage(ItemBase sourceItem, GameObject source, GameObject target, Vector3 position)
    {
        if (!isServer)
        {
            Debug.LogError("Player tried to deal damage instead of server");
            return;
        }

        //todo: crit? if so, what is it?
        //todo: half-damage for duel-weilding
        //todo: give source experience

        if (!target.CompareTag("Player") && !target.CompareTag("Enemy"))
        {
            //Debug.Log("You hit something not damageable");
            return;
        }

        //todo: calc damage
        var defenseStrength = 30;
        var numerator = 100 + _random.Next(0, 10);
        var denominator = 100 + _random.Next(-10, 10);
        var damageDealt = Math.Round(sourceItem.Attributes.Strength * ((double)numerator / (denominator + defenseStrength)), 0);

        Debug.Log($"Source '{sourceItem.Name}' attacked target '{target.name}' for {damageDealt} damage");

        var networkIdentity = SourcePlayer.GetComponent<NetworkIdentity>();
        var controller = SourcePlayer.GetComponent<PlayerController>();
        controller.TargetRpcShowDamage(networkIdentity.connectionToClient, position, damageDealt.ToString(CultureInfo.InvariantCulture));

        //todo: For dealing damage, look at https://docs.unity3d.com/2018.1/Documentation/ScriptReference/Networking.SyncEventAttribute.html
    }

}
