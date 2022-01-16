using System.Collections.Generic;
using FullPotential.Api.Behaviours;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.UI.Components;
using FullPotential.Core.Helpers;
using FullPotential.Core.Networking;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace FullPotential.Core.Behaviours.EnemyBehaviours
{
    public class EnemyState : NetworkBehaviour, IDefensible, IDamageable
    {
        public bool IsDead { get; private set; }

        public readonly NetworkVariable<FixedString32Bytes> EnemyName = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<int> _health = new NetworkVariable<int>(100);
        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();

#pragma warning disable 0649
        [SerializeField] private TextMeshProUGUI _nameTag;
        [SerializeField] private BarSlider _healthSlider;
#pragma warning restore 0649

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _health.OnValueChanged += OnHealthChanged;
            EnemyName.OnValueChanged += OnNameChanged;

            if (IsServer)
            {
                InvokeRepeating(nameof(CheckIfOffTheMap), 1, 1);
            }
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            var values = _healthSlider.GetHealthValues(newValue, GetHealthMax(), GetDefenseValue());
            _healthSlider.SetValues(values);
        }

        private void OnNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            SetName();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SetName();
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

        public void TakeDamage(int amount, ulong? clientId, string attackerName)
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

            if (_health.Value <= 0)
            {
                HandleDeath(attackerName);
            }
        }

        public void HandleDeath(string killerName)
        {
            IsDead = true;

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

            GameManager.Instance.SceneBehaviour.MakeAnnouncementClientRpc($"{name} was killed by {killerName}", RpcHelper.ForNearbyPlayers());

            //todo: Use object pooling
            Destroy(gameObject);

            GameManager.Instance.SceneBehaviour.HandleEnemyDeath();
        }

        private void SetName()
        {
            var variableValue = EnemyName.Value.ToString();
            var displayName = string.IsNullOrWhiteSpace(variableValue)
                ? "Enemy " + NetworkObjectId
                : variableValue;

            gameObject.name = displayName;
            _nameTag.text = displayName;
        }

        private void CheckIfOffTheMap()
        {
            AttackHelper.CheckIfOffTheMap(this, transform.position.y);
        }

    }
}
