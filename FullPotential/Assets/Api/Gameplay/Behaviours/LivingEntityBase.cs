using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Ui.Components;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class LivingEntityBase : NetworkBehaviour
    {
        private const int VelocityThreshold = 4;

        #region Inspector Variables
#pragma warning disable 0649
        // ReSharper disable InconsistentNaming

        [SerializeField] private TextMeshProUGUI _nameTag;

        // ReSharper restore InconsistentNaming
#pragma warning restore 0649
        #endregion

        #region Protected variables
        // ReSharper disable InconsistentNaming

        protected string _lastDamageSourceName;
        protected string _lastDamageItemName;

        protected IGameManager _gameManager;
        protected IRpcService _rpcService;
        protected ILocalizer _localizer;
        protected ITypeRegistry _typeRegistry;
        protected IEffectService _effectService;
        protected IValueCalculator _valueCalculator;

        protected readonly NetworkVariable<FixedString32Bytes> _entityName = new NetworkVariable<FixedString32Bytes>();
        protected readonly NetworkVariable<int> _energy = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _health = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _mana = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _stamina = new NetworkVariable<int>(100);

        // ReSharper restore InconsistentNaming
        #endregion

        #region Other Variables

        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private readonly List<ActiveEffect> _activeEffects = new List<ActiveEffect>();
        private IFighter _fighterWhoMovedMeLast;

        private Rigidbody _rb;
        private bool _isSprinting;

        //Action-related
        private DelayedAction _replenishStamina;
        private DelayedAction _replenishMana;
        private DelayedAction _replenishEnergy;
        private DelayedAction _consumeStamina;

        #endregion

        #region Properties

        protected abstract IStatSlider HealthStatSlider { get; set; }

        public Rigidbody RigidBody => _rb == null ? _rb = GetComponent<Rigidbody>() : _rb;

        public LivingEntityState AliveState { get; protected set; }

        #endregion

        #region Unity Events Handlers
        // ReSharper disable UnusedMemberHierarchy.Global

        protected virtual void Awake()
        {
            _gameManager = ModHelper.GetGameManager();
            _rpcService = _gameManager.GetService<IRpcService>();
            _localizer = _gameManager.GetService<ILocalizer>();
            _typeRegistry = _gameManager.GetService<ITypeRegistry>();
            _effectService = _gameManager.GetService<IEffectService>();
            _valueCalculator = _gameManager.GetService<IValueCalculator>();

            _entityName.OnValueChanged += OnNameChanged;
            _health.OnValueChanged += OnHealthChanged;
            _stamina.OnValueChanged += OnStaminaChanged;
            _energy.OnValueChanged += OnEnergyChanged;
            _mana.OnValueChanged += OnManaChanged;

            if (!NetworkManager.Singleton.IsConnectedClient || IsServer)
            {
                InvokeRepeating(nameof(CheckIfOffTheMap), 1, 1);
            }
        }

        protected virtual void Start()
        {
            AliveState = LivingEntityState.Alive;

            _replenishStamina = new DelayedAction(.01f, () =>
            {
                if (!_isSprinting && _stamina.Value < GetStaminaMax())
                {
                    //todo: zzz v0.5 - trait-based stamina recharge
                    _stamina.Value += 1;
                }
            });

            _replenishMana = new DelayedAction(.2f, () =>
            {
                if (!IsConsumingMana() && _mana.Value < GetManaMax())
                {
                    //todo: zzz v0.5 - trait-based mana recharge
                    _mana.Value += 1;
                }
            });

            _replenishEnergy = new DelayedAction(.2f, () =>
            {
                if (!IsConsumingEnergy() && _energy.Value < GetEnergyMax())
                {
                    //todo: zzz v0.5 - trait-based energy recharge
                    _energy.Value += 1;
                }
            });

            _consumeStamina = new DelayedAction(.05f, () =>
            {
                if (!_isSprinting)
                {
                    return;
                }

                var staminaCost = GetStaminaCost();
                if (_stamina.Value >= staminaCost)
                {
                    _stamina.Value -= staminaCost / 2;
                }
            });
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            UpdateNameOnUi();
        }

        protected virtual void FixedUpdate()
        {
            if (!IsServer)
            {
                return;
            }

            ReplenishAndConsume();
            RemoveExpiredEffects();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _health.OnValueChanged -= OnHealthChanged;
            _stamina.OnValueChanged -= OnStaminaChanged;
            _energy.OnValueChanged -= OnEnergyChanged;
            _mana.OnValueChanged -= OnManaChanged;
        }

        // ReSharper restore UnusedMemberHierarchy.Global
        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void UpdateSprintingServerRpc(bool isSprinting)
        {
            _isSprinting = isSprinting;
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void AddOrUpdateEffectClientRpc(string effectTypeId, int change, DateTime expiry, ClientRpcParams clientRpcParams)
        {
            //Debug.Log("AddOrUpdateEffectClientRpc called with typeId: " + effectTypeId);

            var effect = _typeRegistry.GetEffect(new Guid(effectTypeId));
            AddOrUpdateEffect(effect, change, expiry);
        }

        #endregion

        #region NetworkVariable Handlers

        private void OnNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            UpdateNameOnUi();
        }

        #endregion

        #region Health

        private void ApplyHealthChange(
            int change,
            IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {

            if (sourceFighter == null)
            {
                Debug.LogWarning("Attack source not found. Did they sign out?");
            }
            else
            {
                //Debug.Log($"'{sourceFighter.FighterName}' did {change} health change to '{_entityName.Value}' using '{itemUsed?.Name}'");

                if (change < 0)
                {
                    RecordDamageDealt(change * -1, sourceFighter);
                    ApplyPunchForce(sourceFighter, itemUsed, position);
                }

                ShowHealthChangeToSourceFighter(sourceFighter, position, change);
            }

            //Do this last to ensure the entity does not die before recording the cause
            _health.Value += change;
        }

        private void CheckHealth()
        {
            if (IsServer)
            {
                if (_health.Value <= 0)
                {
                    HandleDeath();
                }

                var healthMax = GetHealthMax();
                if (_health.Value > healthMax)
                {
                    _health.Value = healthMax;
                }
            }
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            UpdateUiHealthAndDefenceValues();
            CheckHealth();
        }

        public int GetHealth()
        {
            CheckHealth();
            return _health.Value;
        }

        public int GetHealthMax()
        {
            //todo: zzz v0.5 - trait-based health max
            return 100 + GetStatMaxAdjustment(AffectableStat.Health);
        }

        #endregion

        #region Stamina

        private void ApplyStaminaChange(int change)
        {
            _stamina.Value += change;
        }

        private void CheckStamina()
        {
            if (IsServer)
            {
                if (_stamina.Value <= 0)
                {
                    //todo: handle no stamina
                }

                var staminaMax = GetStaminaMax();
                if (_stamina.Value > staminaMax)
                {
                    _stamina.Value = staminaMax;
                }
            }
        }

        private void OnStaminaChanged(int previousValue, int newValue)
        {
            CheckStamina();
        }

        public int GetStamina()
        {
            CheckStamina();
            return _stamina.Value;
        }

        public int GetStaminaMax()
        {
            //todo: zzz v0.5 - trait-based stamina max
            return 100 + GetStatMaxAdjustment(AffectableStat.Stamina);
        }

        public int GetStaminaCost()
        {
            //todo: zzz v0.5 - trait-based stamina cost
            return 10;
        }

        #endregion

        #region Energy

        private void ApplyEnergyChange(int change)
        {
            _energy.Value += change;
        }

        private void CheckEnergy()
        {
            if (IsServer)
            {
                if (_energy.Value <= 0)
                {
                    //todo: handle no energy
                }

                var energyMax = GetEnergyMax();
                if (_energy.Value > energyMax)
                {
                    _energy.Value = energyMax;
                }
            }
        }

        private void OnEnergyChanged(int previousValue, int newValue)
        {
            CheckEnergy();
        }

        public int GetEnergy()
        {
            CheckEnergy();
            return _energy.Value;
        }

        public int GetEnergyMax()
        {
            //todo: zzz v0.5 - trait-based energy max
            return 100 + GetStatMaxAdjustment(AffectableStat.Energy);
        }

        protected int GetEnergyCost(Gadget gadget)
        {
            //todo: zzz v0.5 - trait-based energy cost
            return 20;
        }

        protected abstract bool IsConsumingEnergy();

        #endregion

        #region Mana

        private void ApplyManaChange(int change)
        {
            _mana.Value += change;
        }

        private void CheckMana()
        {
            if (IsServer)
            {
                if (_mana.Value <= 0)
                {
                    //todo: handle no mana
                }

                var manaMax = GetManaMax();
                if (_mana.Value > manaMax)
                {
                    _mana.Value = manaMax;
                }
            }
        }
        private void OnManaChanged(int previousValue, int newValue)
        {
            CheckMana();
        }

        public int GetMana()
        {
            CheckMana();
            return _mana.Value;
        }

        public int GetManaMax()
        {
            //todo: zzz v0.5 - trait-based mana max
            return 100 + GetStatMaxAdjustment(AffectableStat.Mana);
        }

        protected int GetManaCost(Spell spell)
        {
            //todo: zzz v0.5 - trait-based mana cost
            return 20;
        }

        protected abstract bool IsConsumingMana();

        #endregion

        #region UI-related Methods

        public void SetName(string newName)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Client tried to set fighter name");
                return;
            }

            _entityName.Value = newName;
        }

        private void UpdateNameOnUi()
        {
            var fighterName = _entityName.Value.ToString();

            var displayName = string.IsNullOrWhiteSpace(fighterName)
                ? "Fighter " + NetworkObjectId
                : fighterName;

            gameObject.name = displayName;
            _nameTag.text = displayName;
        }

        public void UpdateUiHealthAndDefenceValues()
        {
            if (!IsServer && IsOwner)
            {
                return;
            }

            var health = GetHealth();
            var maxHealth = GetHealthMax();
            var defence = GetDefenseValue();
            var values = _gameManager.GetUserInterface().HudOverlay.GetHealthValues(health, maxHealth, defence);
            HealthStatSlider.SetValues(values);
        }

        #endregion

        #region Behaviour-related Methods

        public float GetSprintSpeed()
        {
            //todo: zzz v0.5 - trait-based sprint speed
            return 2.5f;
        }

        private void CheckIfOffTheMap()
        {
            if (AliveState != LivingEntityState.Dead
                && transform.position.y < _gameManager.GetSceneBehaviour().Attributes.LowestYValue)
            {
                _lastDamageItemName = null;
                _lastDamageSourceName = _localizer.Translate("ui.alert.falldamage");
                HandleDeath();
            }
        }

        private void ReplenishAndConsume()
        {
            _replenishStamina.TryPerformAction();
            _replenishMana.TryPerformAction();
            _replenishEnergy.TryPerformAction();

            _consumeStamina.TryPerformAction();
        }

        private void HandleCollision(Collision collision)
        {
            if (collision.relativeVelocity.magnitude < VelocityThreshold)
            {
                _fighterWhoMovedMeLast = null;
                return;
            }

            var normalizedVelocity = collision.relativeVelocity.normalized;

            string cause;
            if (math.abs(normalizedVelocity.y) > math.abs(normalizedVelocity.x)
                && math.abs(normalizedVelocity.y) > math.abs(normalizedVelocity.z))
            {
                cause = _localizer.Translate("ui.alert.falldamage");
            }
            else
            {
                cause = _localizer.Translate("ui.alert.environmentaldamage");
            }

            //Debug.Log($"{name} collided with {collision.gameObject.name} at velocity {collision.relativeVelocity} with cause {cause}");

            var healthChange = _valueCalculator.GetDamageValueFromVelocity(collision.relativeVelocity);
            var position = collision.GetContact(0).point;

            _lastDamageItemName = null;
            _lastDamageSourceName = cause;

            ApplyHealthChange(healthChange, _fighterWhoMovedMeLast, null, position);
        }

        #endregion

        #region Combat-related methods

        public abstract int GetDefenseValue();

        public void SetLastMover(IFighter fighter)
        {
            _fighterWhoMovedMeLast = fighter;
        }

        public void TakeDamageFromFighter(
            IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {
            _lastDamageSourceName = sourceFighter != null ? sourceFighter.FighterName : null;
            _lastDamageItemName = itemUsed?.Name ?? _localizer.Translate("ui.alert.attack.noitem");

            var damageDealt = _valueCalculator.GetDamageValueFromAttack(itemUsed, GetDefenseValue()) * -1;
            ApplyHealthChange(damageDealt, sourceFighter, itemUsed, position);
        }

        public virtual void HandleDeath()
        {
            if (AliveState == LivingEntityState.Dead)
            {
                return;
            }

            AliveState = LivingEntityState.Dead;

            GetComponent<Collider>().enabled = false;

            foreach (var (clientId, _) in _damageTaken)
            {
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
                {
                    continue;
                }

                var playerState = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<IPlayerFighter>();
                playerState.SpawnLootChest(transform.position);
            }

            _damageTaken.Clear();

            var deathMessage = GetDeathMessage(false, name);
            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            _gameManager.GetSceneBehaviour().MakeAnnouncementClientRpc(deathMessage, nearbyClients);

            StopAllCoroutines();
            _activeEffects.Clear();

            HandleDeathAfter(_lastDamageSourceName, _lastDamageItemName);
        }

        protected abstract void HandleDeathAfter(string killerName, string itemName);

        private string GetDeathMessage(bool isOwner, string victimName)
        {
            if (_lastDamageItemName.IsNullOrWhiteSpace())
            {
                return isOwner
                    ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledby"), _lastDamageSourceName)
                    : string.Format(_localizer.Translate("ui.alert.attack.victimkilledby"), victimName, _lastDamageSourceName);
            }

            return isOwner
                ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledbyusing"), _lastDamageSourceName, _lastDamageItemName)
                : string.Format(_localizer.Translate("ui.alert.attack.victimkilledbyusing"), victimName, _lastDamageSourceName, _lastDamageItemName);
        }

        private void RecordDamageDealt(int damageDealt, IFighter sourceFighter)
        {
            var sourceNetworkObject = sourceFighter.GameObject.GetComponent<NetworkObject>();
            var sourceClientId = sourceNetworkObject != null ? (ulong?)sourceNetworkObject.OwnerClientId : null;

            if (sourceClientId != null && !sourceFighter.Equals(this))
            {
                if (_damageTaken.ContainsKey(sourceClientId.Value))
                {
                    _damageTaken[sourceClientId.Value] += damageDealt;
                }
                else
                {
                    _damageTaken.Add(sourceClientId.Value, damageDealt);
                }
            }
        }

        private void ApplyPunchForce(
            IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {
            if (itemUsed == null)
            {
                var targetRb = GetComponent<Rigidbody>();
                if (targetRb != null && position.HasValue)
                {
                    targetRb.AddForceAtPosition(sourceFighter.Transform.forward * 150, position.Value);
                }
            }
        }

        private void ShowHealthChangeToSourceFighter(
            IFighter sourceFighter,
            Vector3? position,
            int change)
        {
            if (sourceFighter.GameObject.CompareTag(Tags.Player)
                && position.HasValue
                && !ReferenceEquals(sourceFighter, this))
            {
                sourceFighter.GameObject.GetComponent<IPlayerBehaviour>().ShowHealthChangeClientRpc(
                    position.Value,
                    change,
                    _rpcService.ForPlayer(sourceFighter.OwnerClientId));
            }
        }

        #endregion

        #region Effect-related methods

        public void AddAttributeModifier(IAttributeEffect attributeEffect, Attributes attributes)
        {
            var (change, expiry) = _valueCalculator.GetAttributeChangeAndExpiry(attributes, attributeEffect);
            AddOrUpdateEffect(attributeEffect, change, expiry);
        }

        public void ApplyPeriodicActionToStat(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter)
        {
            var (change, expiry, delay) = _valueCalculator.GetStatChangeExpiryAndDelay(itemUsed.Attributes, statEffect);
            AddOrUpdateEffect(statEffect, change, expiry);
            StartCoroutine(PeriodicActionToStatCoroutine(statEffect.StatToAffect, change, sourceFighter, itemUsed, delay, expiry));
        }

        private IEnumerator PeriodicActionToStatCoroutine(AffectableStat stat, int change, IFighter sourceFighter, ItemBase itemUsed, float delay, DateTime expiry)
        {
            do
            {
                ApplyStatChange(stat, change, sourceFighter, itemUsed, transform.position);
                yield return new WaitForSeconds(delay);

            } while (DateTime.Now < expiry);
        }

        public void ApplyStatValueChange(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            var (change, expiry) = _valueCalculator.GetStatChangeAndExpiry(itemUsed.Attributes, statEffect);

            if (statEffect.StatToAffect == AffectableStat.Health && statEffect.Affect == Affect.SingleDecrease)
            {
                change = _valueCalculator.GetDamageValueFromAttack(itemUsed, GetDefenseValue()) * -1;
            }

            AddOrUpdateEffect(statEffect, change, expiry);

            ApplyStatChange(statEffect.StatToAffect, change, sourceFighter, itemUsed, position);
        }

        public void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            var (change, expiry) = _valueCalculator.GetStatChangeAndExpiry(itemUsed.Attributes, statEffect);

            AddOrUpdateEffect(statEffect, change, expiry);

            ApplyStatChange(statEffect.StatToAffect, change, sourceFighter, itemUsed, position);
        }

        public void ApplyElementalEffect(IEffect elementalEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            TakeDamageFromFighter(sourceFighter, itemUsed, position);

            //todo: ApplyElementalEffect
            //Debug.LogWarning("Not yet implemented elemental effects");
        }

        public List<ActiveEffect> GetActiveEffects()
        {
            return _activeEffects;
        }

        protected int GetAttributeAdjustment(AffectableAttribute attribute)
        {
            return _activeEffects
                .Where(x =>
                    x.Effect is IAttributeEffect attributeEffect
                    && attributeEffect.AttributeToAffect == attribute)
                .Sum(x => x.Change * (x.Effect is IAttributeEffect attributeEffect && attributeEffect.TemporaryMaxIncrease ? 1 : -1));
        }

        private int GetStatMaxAdjustment(AffectableStat affectableStat)
        {
            return _activeEffects
                .Where(x =>
                    x.Effect is IStatEffect statEffect
                    && statEffect.StatToAffect == affectableStat
                    && statEffect.Affect is Affect.TemporaryMaxIncrease or Affect.TemporaryMaxDecrease)
                .Sum(x => x.Change);
        }

        private bool DoesAffectAllowMultiple(Affect affect)
        {
            return affect == Affect.TemporaryMaxIncrease
                || affect == Affect.TemporaryMaxDecrease;
        }

        private void AddOrUpdateEffect(IEffect effect, int change, DateTime expiry)
        {
            var multipleAllowed = (effect is IStatEffect statEffect && DoesAffectAllowMultiple(statEffect.Affect))
                || effect is IAttributeEffect;

            var effectMatch = _activeEffects.FirstOrDefault(x => x.Effect == effect);

            if (effectMatch != null)
            {
                if (multipleAllowed)
                {
                    _activeEffects.Add(new ActiveEffect
                    {
                        Id = Guid.NewGuid(),
                        Effect = effect,
                        Change = change,
                        Expiry = expiry
                    });
                }
                else
                {
                    effectMatch.Change = change;
                    effectMatch.Expiry = expiry;
                }
            }
            else
            {
                _activeEffects.Add(new ActiveEffect
                {
                    Id = Guid.NewGuid(),
                    Effect = effect,
                    Change = change,
                    Expiry = expiry
                });
            }

            if (OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                AddOrUpdateEffectClientRpc(effect.TypeId.ToString(), change, expiry, _rpcService.ForPlayer(OwnerClientId));
            }
        }

        private void ApplyStatChange(
            AffectableStat stat,
            int change,
            IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {
            switch (stat)
            {
                case AffectableStat.Energy:
                    ApplyEnergyChange(change);
                    return;

                case AffectableStat.Health:
                    ApplyHealthChange(change, sourceFighter, itemUsed, position);
                    return;

                case AffectableStat.Mana:
                    ApplyManaChange(change);
                    return;

                case AffectableStat.Stamina:
                    ApplyStaminaChange(change);
                    return;

                default:
                    throw new ArgumentException("Unexpected AffectableStat: " + stat);
            }
        }

        private void RemoveExpiredEffects()
        {
            //Note: .ToList() needed to avoid the "Collection was modified" exception
            var expiredEffects = _activeEffects
                .Where(x => x.Expiry < DateTime.Now)
                .ToList();

            foreach (var activeEffect in expiredEffects)
            {
                _activeEffects.Remove(activeEffect);
            }
        }

        //private int GetStatVariableMax(AffectableStat stat)
        //{
        //    switch (stat)
        //    {
        //        case AffectableStat.Energy: return GetEnergyMax();
        //        case AffectableStat.Health: return GetHealthMax();
        //        case AffectableStat.Mana: return GetManaMax();
        //        case AffectableStat.Stamina: return GetStaminaMax();
        //        default:
        //            throw new ArgumentException("Unexpected AffectableStat: " + stat);
        //    }
        //}

        #endregion
    }
}
