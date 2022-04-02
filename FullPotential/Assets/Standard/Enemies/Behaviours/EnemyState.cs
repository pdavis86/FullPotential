using System.Collections.Generic;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Ui.Components;
using FullPotential.Api.Utilities;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyState : NetworkBehaviour, IEnemyStateBehaviour
    {
        public LivingEntityState AliveState { get; private set; }

#pragma warning disable 0649
        [SerializeField] private TextMeshProUGUI _nameTag;
        [SerializeField] private GameObject _healthSliderParent;
#pragma warning restore 0649

        public readonly NetworkVariable<FixedString32Bytes> EnemyName = new NetworkVariable<FixedString32Bytes>();

        private IGameManager _gameManager;
        private IAttackHelper _attackHelper;
        private IRpcHelper _rpcHelper;
        //private IEffectHelper _effectHelper;

        private readonly NetworkVariable<int> _health = new NetworkVariable<int>(100);
        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private IStatSlider _healthSlider;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _gameManager = ModHelper.GetGameManager();
            _attackHelper = _gameManager.GetService<IAttackHelper>();
            _rpcHelper = _gameManager.GetService<IRpcHelper>();
            //_effectHelper = _gameManager.GetService<IEffectHelper>();

            _healthSlider = _healthSliderParent.GetComponent<IStatSlider>();

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

        //todo: attribute-based defense
        public int GetDefenseValue()
        {
            return 50;
        }

        //todo: attribute-based health
        public int GetHealthMax()
        {
            return 100;
        }

        public int GetHealth()
        {
            return _health.Value;
        }

        public void TakeDamage(int amount, ulong? clientId, string attackerName, string itemName)
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
                HandleDeath(attackerName, itemName);
            }
        }

        public void HandleDeath(string killerName, string itemName)
        {
            AliveState = LivingEntityState.Dead;

            GetComponent<Collider>().enabled = false;

            foreach (var item in _damageTaken)
            {
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(item.Key))
                {
                    continue;
                }
                var playerState = NetworkManager.Singleton.ConnectedClients[item.Key].PlayerObject.gameObject.GetComponent<IPlayerStateBehaviour>();
                playerState.SpawnLootChest(transform.position);
            }

            _damageTaken.Clear();

            var deathMessage = _attackHelper.GetDeathMessage(false, name, killerName, itemName);
            var nearbyClients = _rpcHelper.ForNearbyPlayers(transform.position);
            _gameManager.GetSceneBehaviour().MakeAnnouncementClientRpc(deathMessage, nearbyClients);

            Destroy(gameObject);

            _gameManager.GetSceneBehaviour().HandleEnemyDeath();
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
            _attackHelper.CheckIfOffTheMap(this, transform.position.y);
        }

        public NetworkVariable<int> GetStatVariable(AffectableStats stat)
        {
            //todo:
            return null;
        }

        public int GetStatVariableMax(AffectableStats stat)
        {
            //todo:
            return -1;
        }

        //todo: move these
        public void AddAttributeModifier(IAttributeEffect attributeEffect, Attributes attributes)
        {
            throw new System.NotImplementedException();
        }

        public void ApplyPeriodicActionToStat(IStatEffect statEffect, Attributes attributes)
        {
            //todo:
            throw new System.NotImplementedException();
        }

        public void AlterValue(IStatEffect statEffect, Attributes attributes)
        {
            //todo:
            throw new System.NotImplementedException();
        }

        public void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, Attributes attributes)
        {
            //todo:
            throw new System.NotImplementedException();
        }

        public Rigidbody GetRigidBody()
        {
            //todo:
            throw new System.NotImplementedException();
        }
    }
}
