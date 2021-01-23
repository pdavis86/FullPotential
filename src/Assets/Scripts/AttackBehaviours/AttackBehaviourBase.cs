﻿using Assets.Scripts.Attributes;
using Assets.Scripts.Crafting.Results;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Attacks
{
    public abstract class AttackBehaviourBase : MonoBehaviour
    {
        public GameObject Player;
        public Camera PlayerCamera;
        public GameObject HitTextUiPrefab;
        public Transform DamageNumbersParent;

        private static readonly System.Random _random = new System.Random();

        [ServerSideOnlyTemp]
        internal void DealDamage(CraftableBase source, GameObject target, Vector3 position)
        {
            //todo: check target is damagable
            //todo: crit? if so, what is it?

            //todo: calc damage
            var defenseStrength = 30;
            var numerator = 100 + _random.Next(0, 10);
            var denominator = 100 + _random.Next(-10, 10);
            var damageDealt = Math.Round(source.Attributes.Strength * ((double)numerator / (denominator + defenseStrength)), 0);

            //Debug.Log($"Source '{source.Name}' attacked target '{target.name}' for {damageDealt} damage");

            ShowDamage(position, damageDealt.ToString());
        }

        [ClientSideOnlyTemp]
        private void ShowDamage(Vector3 position, string damage)
        {
            var hit = Instantiate(HitTextUiPrefab);
            hit.transform.SetParent(DamageNumbersParent, false);
            hit.gameObject.SetActive(true);

            var hitText = hit.GetComponent<TextMeshProUGUI>();
            hitText.text = damage.ToString();

            var sticky = hit.GetComponent<StickToWorldPosition>();
            sticky.PlayerCamera = PlayerCamera;
            sticky.WorldPosition = position;

            var ttl = hit.gameObject.AddComponent<TimeToLive>();
            ttl.GameObjectToDestroy = hit.gameObject;
            ttl.AllowedTime = 1f;
        }

    }
}