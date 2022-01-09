using System.Collections.Generic;
using FullPotential.Api.Behaviours;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Slider _healthSlider;
#pragma warning restore 0649

        private void Awake()
        {
            _health.OnValueChanged += OnHealthChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            transform.Find("Graphics").Find("Canvas").Find("NameTag").GetComponent<TextMeshProUGUI>().text = EnemyName.Value.ToString();
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

        private void OnHealthChanged(int previousValue, int newValue)
        {
            _healthSlider.value = (float)newValue / GetHealthMax();
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

    }
}
