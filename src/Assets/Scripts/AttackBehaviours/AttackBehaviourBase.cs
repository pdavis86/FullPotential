using Assets.Scripts.Crafting.Results;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable NotAccessedField.Global

public abstract class AttackBehaviourBase : NetworkBehaviour
{
    public GameObject SourcePlayer;

    // ReSharper disable once InconsistentNaming
    private static readonly System.Random _random = new System.Random();

    private SceneObjects001 _sceneObjects;
    private SceneObjects001 GetSceneObjects()
    {
        return _sceneObjects ?? (_sceneObjects = GameObject.Find("SceneObjects").GetComponent<SceneObjects001>());
    }

    [ServerCallback]
    internal void DealDamage(ItemBase sourceItem, GameObject source, GameObject target, Vector3 position)
    {
        //if (!isServer)
        //{
        //    //Debug.LogError("Player tried to deal damage instead of server");
        //    return;
        //}

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

        //Debug.Log($"Source '{source.Name}' attacked target '{target.name}' for {damageDealt} damage");

        var networkIdentity = SourcePlayer.GetComponent<NetworkIdentity>();
        TargetRpcShowDamage(networkIdentity.connectionToClient, position, damageDealt.ToString(CultureInfo.InvariantCulture));


        //For dealing damage, look at https://docs.unity3d.com/2018.1/Documentation/ScriptReference/Networking.SyncEventAttribute.html

        //Don't destroy the object or the RPC will fail. Destroy() is handled in Awake();
        //NetworkServer.Destroy(source);
    }

    // ReSharper disable once UnusedParameter.Local
    [TargetRpc]
    private void TargetRpcShowDamage(NetworkConnection playerConnection, Vector3 position, string damage)
    {
        var hit = Instantiate(GetSceneObjects().PrefabHitText);
        hit.transform.SetParent(GetSceneObjects().UiHitNumbers.transform, false);
        hit.gameObject.SetActive(true);

        var hitText = hit.GetComponent<TextMeshProUGUI>();
        hitText.text = damage;

        const int maxDistanceForMinFontSize = 40;
        var distance = Vector3.Distance(Camera.main.transform.position, position);
        var fontSize = maxDistanceForMinFontSize - distance;
        if (fontSize < hitText.fontSizeMin) { fontSize = hitText.fontSizeMin; }
        else if (fontSize > hitText.fontSizeMax) { fontSize = hitText.fontSizeMax; }
        hitText.fontSize = fontSize;

        var sticky = hit.GetComponent<StickUiToWorldPosition>();
        sticky.WorldPosition = position;

        Destroy(hit, 1f);
    }

}
