using System.Collections.Generic;
using FullPotential.Api.Behaviours;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
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
        private const int _maxHealth = 100;

        private readonly NetworkVariable<int> _health = new NetworkVariable<int>(_maxHealth);
        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();

#pragma warning disable 0649
        [SerializeField] private Slider _healthSlider;
#pragma warning restore 0649

        private void Awake()
        {
            _health.OnValueChanged += OnHealthChanged;
        }

        public int GetDefenseValue()
        {
            //todo: implement enemy GetDefenseValue()
            return 50;
        }

        public int GetHealthMax()
        {
            return _maxHealth;
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
            _healthSlider.value = (float)newValue / _maxHealth;
        }

        public void HandleDeath()
        {
            GetComponent<Collider>().enabled = false;

            foreach (var item in _damageTaken)
            {
                var playerState = NetworkManager.Singleton.ConnectedClients[item.Key].PlayerObject.gameObject.GetComponent<PlayerState>();
                playerState.SpawnLootChest(transform.position, transform.rotation);
            }

            Destroy(gameObject);

            GameManager.Instance.SceneObjects.GetComponent<ISceneEvents>().OnEnemyDeath();
        }

    }
}
