using System.Collections.Generic;
using FullPotential.Api.Behaviours;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.UI.Components;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace FullPotential.Core.Behaviours.EnemyBehaviours
{
    public class EnemyState : NetworkBehaviour, IDefensible, IDamageable
    {
        public readonly NetworkVariable<FixedString32Bytes> EnemyName = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<int> _health = new NetworkVariable<int>(100);
        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();

#pragma warning disable 0649
        [SerializeField] private HealthSlider _healthSlider;
#pragma warning restore 0649

        private void Awake()
        {
            _health.OnValueChanged += OnHealthChanged;
            EnemyName.OnValueChanged += OnNameChanged;
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            _healthSlider.SetValue(newValue, GetHealthMax(), GetDefenseValue());
        }

        private void OnNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            SetNameTag();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SetNameTag();
        }

        public int GetDefenseValue()
        {
            return 50;
        }

        public int GetHealthMax()
        {
            return 100;
        }

        public int GetHealth()
        {
            return _health.Value;
        }

        public void TakeDamage(ulong? clientId, int amount)
        {
            if (clientId != null)
            {
                if (_damageTaken.ContainsKey(clientId.Value))
                {
                    _damageTaken[clientId.Value] += amount;
                }
                else
                {
                    _damageTaken.Add(clientId.Value, amount);
                }
            }

            _health.Value -= amount;
        }

        public void HandleDeath()
        {
            GetComponent<Collider>().enabled = false;

            foreach (var item in _damageTaken)
            {

                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(item.Key))
                {
                    continue;
                }
                var playerState = NetworkManager.Singleton.ConnectedClients[item.Key].PlayerObject.gameObject.GetComponent<PlayerState>();
                playerState.SpawnLootChest(transform.position);
            }

            _damageTaken.Clear();

            //todo: Use object pooling
            Destroy(gameObject);

            GameManager.Instance.SceneBehaviour.OnEnemyDeath();
        }

        private void SetNameTag()
        {
            transform.Find("Graphics").Find("Canvas").Find("NameTag").GetComponent<TextMeshProUGUI>().text = EnemyName.Value.ToString();
        }

    }
}
