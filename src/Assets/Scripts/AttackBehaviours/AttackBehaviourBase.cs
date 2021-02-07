using Assets.Scripts.Attributes;
using Assets.Scripts.Crafting.Results;
using Assets.Scripts.Networking;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable NotAccessedField.Global

public abstract class AttackBehaviourBase : MonoBehaviour
{
    // ReSharper disable once InconsistentNaming
    private static readonly System.Random _random = new System.Random();

    [ServerSideOnly]
    internal void CmdDealDamage(ItemBase source, GameObject target, Vector3 position)
    {
        //todo: crit? if so, what is it?
        //todo: half-damage for duel-weilding

        if (!target.CompareTag("Player") && !target.CompareTag("Enemy"))
        {
            //Debug.Log("You hit something not damageable");
            return;
        }

        //todo: calc damage
        var defenseStrength = 30;
        var numerator = 100 + _random.Next(0, 10);
        var denominator = 100 + _random.Next(-10, 10);
        var damageDealt = Math.Round(source.Attributes.Strength * ((double)numerator / (denominator + defenseStrength)), 0);

        //Debug.Log($"Source '{source.Name}' attacked target '{target.name}' for {damageDealt} damage");

        RpcShowDamage(position, damageDealt.ToString(CultureInfo.InvariantCulture));
    }

    [ClientSideFromServer]
    private void RpcShowDamage(Vector3 position, string damage)
    {
        var hit = Instantiate(GameManager.Instance.GameObjects.PrefabHitText);
        hit.transform.SetParent(GameManager.Instance.GameObjects.UiDamageNumbers.transform, false);
        hit.gameObject.SetActive(true);

        var hitText = hit.GetComponent<TextMeshProUGUI>();
        hitText.text = damage;

        var sticky = hit.GetComponent<StickToWorldPosition>();
        sticky.WorldPosition = position;

        Destroy(hit, 1f);
    }

}
