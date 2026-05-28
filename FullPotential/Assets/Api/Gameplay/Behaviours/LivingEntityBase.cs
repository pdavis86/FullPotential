using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Data.Models;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Scenes;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;

using TMPro;

using Unity.Collections;
using Unity.Netcode;

using UnityEngine;

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable MemberCanBePrivate.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class LivingEntityBase : NetworkBehaviour
    {
        public const string ResourceValueChangeEventId = "34372a74-abf3-44eb-8598-4427a82f29ab";

        private const int VelocityThreshold = 3;
        private const int ForceThreshold = 1000;
        private const int SingleResourceChangeEffectDisplaySeconds = 3;

        #region Inspector Variables
#pragma warning disable 0649
        // ReSharper disable InconsistentNaming

        // ReSharper disable UnassignedField.Global
        [SerializeField] private TextMeshProUGUI _nameTag;
        [SerializeField] protected Transform _graphicsTransform;
        // ReSharper restore UnassignedField.Global

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
        protected ICombatService _combatService;
        protected IEventBus _eventBus;
        protected ISceneService _sceneService;

        protected readonly NetworkVariable<FixedString128Bytes> _entityName = new NetworkVariable<FixedString128Bytes>();

        protected InventoryBase _inventory;

        // ReSharper restore InconsistentNaming
        #endregion

        #region Other Variables

        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private readonly List<ActiveEffect> _activeEffects = new List<ActiveEffect>();
        private readonly Dictionary<string, int> _resourceValueCache = new Dictionary<string, int>();

        private CancellationTokenSource _activeEffectsCancellationTokenSource = new CancellationTokenSource();
        private IEnumerable<IResourceType> _sortedResources;

        private FighterBase _fighterWhoMovedMeLast;
        private Rigidbody _rb;

        //Action-related
        private DelayedAction _replenishResources;
        private DelayedAction _consumeStamina;

        #endregion

        #region Properties

        protected abstract IBarSlider HealthBarSlider { get; set; }

        public Rigidbody RigidBody => _rb == null ? _rb = GetComponent<Rigidbody>() : _rb;

        public LivingEntityState AliveState { get; protected set; }

        public bool IsSprinting { get; set; }

        public InventoryBase Inventory => _inventory;

        #endregion

        #region Unity Events Handlers
        // ReSharper disable UnusedMemberHierarchy.Global

        protected virtual void Awake()
        {
            _gameManager = DependenciesContext.Dependencies.GetService<IGameManager>();
            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();
            _eventBus = DependenciesContext.Dependencies.GetService<IEventBus>();
            _sceneService = _gameManager.GetSceneBehaviour().GetSceneService();

            PopulateResourceValueCache();

            _entityName.OnValueChanged += HandleNameChange;
        }

        protected virtual void Start()
        {
            AliveState = LivingEntityState.Alive;

            SetupResourceReplenishing();
            SetupStaminaConsumption();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                InvokeRepeating(nameof(CheckIfOffTheMap), 1, 1);
            }

            UpdateNameOnUi();
        }

        protected virtual void FixedUpdate()
        {
            RemoveExpiredEffects();
            ReplenishAndConsume();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision);
        }

        public override void OnDestroy()
        {
            _entityName.OnValueChanged -= HandleNameChange;

            base.OnDestroy();
        }

        // ReSharper restore UnusedMemberHierarchy.Global
        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void UpdateHealthValueClientRpc(int newValue, ClientRpcParams clientRpcParams)
        {
            UpdateResourceValue(ResourceTypeIds.HealthId, newValue);
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void AddOrUpdateEffectClientRpc(string effectTypeId, int change, DateTime expiry, ClientRpcParams clientRpcParams)
        {
            //Debug.Log("AddOrUpdateEffectClientRpc called with typeId: " + effectTypeId);

            var effect = _typeRegistry.GetRegisteredByTypeId<IEffectType>(effectTypeId);
            AddOrUpdateEffect(effect, change, expiry);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        protected void ShowHudAlertClientRpc(string announcement, ClientRpcParams clientRpcParams)
        {
            if (announcement.IsNullOrWhiteSpace())
            {
                return;
            }

            _gameManager.GetUserInterface().HudOverlay.ShowAlert(announcement);
        }

        #endregion

        #region NetworkVariable Handlers

        private void HandleNameChange(FixedString128Bytes previousValue, FixedString128Bytes newValue)
        {
            UpdateNameOnUi();
        }

        #endregion

        #region Resource Management

        protected IEnumerable<IResourceType> GetResources()
        {
            return _sortedResources;
        }

        protected Dictionary<string, int> GetResourceDictionaryForSave()
        {
            return GetResources().ToDictionary(
                x => x.TypeId.ToString(), 
                x => GetResourceValue(x.TypeId.ToString()));
        }

        private void SetupResourceReplenishing()
        {
            //todo: zzz v0.8 - trait-based resource replenish (not just every .2 seconds)
            _replenishResources = new DelayedAction(.2f, () =>
            {
                foreach (var resource in GetResources())
                {
                    resource.ReplenishBehaviour?.Invoke(this);
                }
            });
        }

        public void SetLastDamageValues(FighterBase sourceFighter, CombatItemBase itemUsed, int change)
        {
            _lastDamageSourceName = sourceFighter != null ? sourceFighter.FighterName : null;
            _lastDamageItemName = itemUsed?.Name.OrIfNullOrWhitespace(_localizer.Translate("ui.alert.attack.noitem"));

            //Debug.Log($"'{sourceFighter.FighterName}' did {change} health change to '{_entityName.Value}' using '{itemUsed?.Name}'");

            if (sourceFighter == null)
            {
                return;
            }

            var sourceNetworkObject = sourceFighter.GameObject.GetComponent<NetworkObject>();
            var sourceClientId = sourceNetworkObject != null ? (ulong?)sourceNetworkObject.OwnerClientId : null;

            if (sourceClientId != null && !sourceFighter.Equals(this))
            {
                var damageDealt = change * -1;

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

        private void PopulateResourceValueCache()
        {
            _sortedResources = _typeRegistry.GetRegisteredTypes<IResourceType>()
                .OrderBy(x => x.TypeId);

            foreach (var resource in _sortedResources)
            {
                _resourceValueCache.Add(resource.TypeId.ToString(), 9999);
            }
        }

        public int GetResourceValue(string typeId)
        {
            return _resourceValueCache[typeId];
        }

        protected int ClampResourceValue(string typeId, int value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else
            {
                var resourceMax = GetResourceMax(typeId);

                if (value > resourceMax)
                {
                    value = resourceMax;
                }
            }

            return value;
        }

        protected void SetResourceInitialValues(Dictionary<string, int> values)
        {
            foreach (var kvp in values)
            {
                _resourceValueCache[kvp.Key] = ClampResourceValue(kvp.Key, kvp.Value);
            }

            UpdateUiHealthAndDefenceValues();
        }

        public void TriggerResourceValueUpdate(string typeId, int oldValue, int newValue)
        {
            TriggerResourceValueUpdate(typeId, newValue - oldValue);
        }

        public void TriggerResourceValueUpdate(string typeId, int change)
        {
            var currentValue = ClampResourceValue(typeId, GetResourceValue(typeId));
            var eventArgs = new ResourceValueChangedEventArgs(this, typeId, currentValue + change, change);
            _eventBus.PublishAsync(ResourceValueChangeEventId, eventArgs).Forget();
        }

        public static UniTask DefaultHandlerForResourceValueChangeEventAsync(ResourceValueChangedEventArgs eventArgs)
        {
            eventArgs.LivingEntity.UpdateResourceValue(eventArgs.ResourceTypeId, eventArgs.NewValue);
            return UniTask.CompletedTask;
        }

        // todo: remove debugging
        //private string GetResourceTypeName(string resourceTypeId)
        //{
        //    switch (resourceTypeId)
        //    {
        //        case ResourceTypeIds.HealthId: return "Health";
        //        case ResourceTypeIds.StaminaId: return "Stamina";
        //        case "378443ee-7942-4cd5-977d-818ee03333e9": return "Mana";
        //        case "89ec3ecf-badb-4e55-91b0-b288ca358010": return "Energy";
        //        case "9f026e17-d313-4402-9da6-c5b002e26c64": return "Barrier charge";
        //        default: return "Unknown";
        //    }
        //}

        internal void UpdateResourceValue(string typeId, int newValue)
        {
            newValue = ClampResourceValue(typeId, newValue);

            // todo: remove debugging
            //var locationName = IsServer ? "Server" : "Client";
            //var typeName = GetResourceTypeName(typeId);
            //Debug.Log($"{locationName}-{OwnerClientId}: '{typeName}' changed from {_resourceValueCache[typeId]} to {newValue}");

            _resourceValueCache[typeId] = newValue;

            // todo: zzz v0.7 - remove health bar over each player?
            if (IsServer && typeId == ResourceTypeIds.HealthId)
            {
                var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, 0);
                UpdateHealthValueClientRpc(newValue, nearbyClients);
            }
        }

        protected void SetResourceValuesForRespawn()
        {
            var resourceKeys = _resourceValueCache.Keys.ToList();
            foreach (var resourceTypeId in resourceKeys)
            {
                TriggerResourceValueUpdate(resourceTypeId, GetResourceValue(resourceTypeId), GetResourceMax(resourceTypeId));
            }
        }

        public int GetResourceMax(string resourceTypeId)
        {
            //todo: zzz v0.8 - trait-based resource max
            return 100 + GetResourceMaxAdjustment(resourceTypeId);
        }

        public abstract bool IsConsumingResource(string typeId);

        #endregion

        #region Sprint-specific

        public int GetStaminaCost()
        {
            //todo: zzz v0.8 - trait-based sprint costs
            return 10;
        }

        public float GetSprintSpeed()
        {
            //todo: zzz v0.8 - trait-based sprint speed
            return 2.5f;
        }

        protected bool IsConsumingStamina()
        {
            if (IsSprinting && GetResourceValue(ResourceTypeIds.StaminaId) < GetStaminaCost())
            {
                IsSprinting = false;
            }

            return IsSprinting;
        }

        private void SetupStaminaConsumption()
        {
            _consumeStamina = new DelayedAction(.05f, () =>
            {
                if (!IsConsumingStamina())
                {
                    return;
                }

                var staminaValue = GetResourceValue(ResourceTypeIds.StaminaId);
                var staminaCost = GetStaminaCost();
                if (staminaValue >= staminaCost)
                {
                    TriggerResourceValueUpdate(ResourceTypeIds.StaminaId, -staminaCost / 2);
                }
            });
        }

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
            if (!IsClient)
            {
                return;
            }

            var health = GetResourceValue(ResourceTypeIds.HealthId);
            var maxHealth = GetResourceMax(ResourceTypeIds.HealthId);
            var values = _gameManager.GetUserInterface().HudOverlay.GetSliderBarValues(health, maxHealth, null);
            HealthBarSlider.UpdateValues(values.text, values.percent, 1);
        }

        #endregion

        #region Behaviour-related Methods

        private void CheckIfOffTheMap()
        {
            if (AliveState == LivingEntityState.Alive
                && transform.position.y < _gameManager.GetSceneBehaviour().Attributes.LowestYValue)
            {
                _lastDamageItemName = null;
                _lastDamageSourceName = _localizer.Translate("ui.alert.falldamage");
                HandleDeath();
            }
        }

        private void ReplenishAndConsume()
        {
            _replenishResources.TryPerformAction();
            _consumeStamina.TryPerformAction();
        }

        private void HandleCollision(Collision collision)
        {
            if (!IsServer)
            {
                return;
            }

            var contactPoint = collision.GetContact(0);

            var force = collision.impulse / Time.fixedDeltaTime;
            var isForceDamage = force.magnitude >= ForceThreshold;

            var velocityMagnitude = collision.relativeVelocity.magnitude;
            var isVelocityDamage = velocityMagnitude >= VelocityThreshold;

            if (!isVelocityDamage && !isForceDamage)
            {
                _fighterWhoMovedMeLast = null;
                return;
            }

            var cause = _localizer.Translate(contactPoint.normal == Vector3.up
                ? "ui.alert.falldamage"
                : "ui.alert.environmentaldamage");

            //Debug.Log($"{name} collided with {collision.gameObject.name} at velocity {collision.relativeVelocity} with force {force} with cause {cause}");

            var healthChangeRaw = isVelocityDamage
                ? Vector3.Dot(contactPoint.normal, collision.relativeVelocity)
                : force.magnitude / 700;

            var healthChange = -1 * (int)MathF.Round(healthChangeRaw, MidpointRounding.AwayFromZero);

            _lastDamageItemName = null;
            _lastDamageSourceName = cause;

            SetLastDamageValues(_fighterWhoMovedMeLast, null, healthChange);

            if (_fighterWhoMovedMeLast != null)
            {
                ShowHealthChangeToSourceFighter(_fighterWhoMovedMeLast, contactPoint.point, healthChange, false);
            }

            TriggerResourceValueUpdate(ResourceTypeIds.HealthId, healthChange);
        }

        #endregion

        #region Combat-related methods

        public static int GetAdjustedStrength(int strength)
        {
            const float strengthDivisor = 4;
            return (int)Math.Ceiling(strength / strengthDivisor);
        }

        public void SetLastMover(FighterBase fighter)
        {
            _fighterWhoMovedMeLast = fighter;
        }

        public virtual void HandleDeath()
        {
            if (AliveState == LivingEntityState.Dead)
            {
                return;
            }

            AliveState = LivingEntityState.Dead;

            _graphicsTransform.gameObject.SetActive(false);

            GetComponent<Collider>().enabled = false;

            var lootPosition = _sceneService.GetPositionOnSolidObject(transform.position);

            foreach (var (clientId, _) in _damageTaken)
            {
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
                {
                    continue;
                }

                var playerState = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<IPlayerFighter>();
                playerState.SpawnLootChest(lootPosition);
            }

            _damageTaken.Clear();

            var deathMessage = GetDeathMessage(name);
            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            ShowHudAlertClientRpc(deathMessage, nearbyClients);

            _activeEffectsCancellationTokenSource.Cancel();
            _activeEffectsCancellationTokenSource.Dispose();
            _activeEffectsCancellationTokenSource = new CancellationTokenSource();

            _activeEffects.Clear();

            HandleDeathAfter();
        }

        protected abstract void HandleDeathAfter();

        private string GetDeathMessage(string victimName)
        {
            if (gameObject == _gameManager.GetLocalPlayerGameObject())
            {
                if (_lastDamageSourceName == victimName)
                {
                    return _lastDamageItemName.IsNullOrWhiteSpace()
                        ? _localizer.Translate("ui.alert.attack.youkilledyourself")
                        : _localizer.Translate("ui.alert.attack.youkilledyourselfusing", _lastDamageItemName);
                }

                return _lastDamageItemName.IsNullOrWhiteSpace()
                    ? _localizer.Translate("ui.alert.attack.youwerekilledby", _lastDamageSourceName)
                    : _localizer.Translate("ui.alert.attack.youwerekilledbyusing", _lastDamageSourceName, _lastDamageItemName);
            }

            if (_lastDamageSourceName == victimName)
            {
                return _lastDamageItemName.IsNullOrWhiteSpace()
                    ? _localizer.Translate("ui.alert.attack.victimsuicide", victimName)
                    : _localizer.Translate("ui.alert.attack.victimsuicideusing", victimName, _lastDamageItemName);
            }

            return _lastDamageItemName.IsNullOrWhiteSpace()
                ? _localizer.Translate("ui.alert.attack.victimkilledby", victimName, _lastDamageSourceName)
                : _localizer.Translate("ui.alert.attack.victimkilledbyusing", victimName, _lastDamageSourceName, _lastDamageItemName);
        }

        public void ShowHealthChangeToSourceFighter(
            FighterBase sourceFighter,
            Vector3? position,
            int change,
            bool isCritical)
        {
            if (sourceFighter.GameObject.CompareTag(Tags.Player)
                && position.HasValue
                && !ReferenceEquals(sourceFighter, this))
            {
                sourceFighter.GameObject.GetComponent<IPlayerBehaviour>().ShowHealthChangeClientRpc(
                    position.Value,
                    change,
                    isCritical,
                    _rpcService.ForPlayer(sourceFighter.OwnerClientId));
            }
        }

        #endregion

        #region Effect-related methods

        public void AddAttributeModifier(IAttributeEffect attributeEffect, int change, DateTime expiry, Vector3? position)
        {
            AddOrUpdateEffect(attributeEffect, change, expiry);
        }

        public void ApplyPeriodicActionToResource(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffectType resourceEffect, Vector3? position)
        {
            // todo: zzz v0.6 - replace use of DateTime with TimeProvider to ensure UTC
            var delay = itemUsed.GetChargeUpTime();
            var expiry = DateTime.Now.AddSeconds(itemUsed.GetEffectDuration());
            PeriodicActionToResourceAsync(sourceFighter, itemUsed, resourceEffect, position, delay, expiry, _activeEffectsCancellationTokenSource.Token).Forget();
        }

        private async UniTask PeriodicActionToResourceAsync(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffectType resourceEffect, Vector3? position, float delay, DateTime expiry, CancellationToken cancellationToken)
        {
            do
            {
                ApplySingleValueChangeToResourceInternal(sourceFighter, itemUsed, resourceEffect, position);
                await UniTask.WaitForSeconds(delay, cancellationToken: cancellationToken);

            } while (DateTime.Now < expiry);
        }

        public void ApplySingleValueChangeToResource(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffectType resourceEffect, Vector3? position)
        {
            ApplySingleValueChangeToResourceInternal(sourceFighter, itemUsed, resourceEffect, position);
        }

        private void ApplySingleValueChangeToResourceInternal(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffectType resourceEffect, Vector3? position)
        {
            var combatResult = _combatService.GetCombatResult(sourceFighter, itemUsed, resourceEffect, this);

            if (resourceEffect.ResourceTypeIdString == ResourceTypeIds.HealthId)
            {
                if (combatResult.Change < 0)
                {
                    SetLastDamageValues(sourceFighter, itemUsed, combatResult.Change);
                }

                //todo: zzz v0.7 - generalise so other types of change can be shown
                if (sourceFighter != null)
                {
                    ShowHealthChangeToSourceFighter(sourceFighter, position, combatResult.Change, combatResult.IsCriticalHit);
                }
            }

            TriggerResourceValueUpdate(resourceEffect.ResourceTypeIdString, combatResult.Change);
            AddOrUpdateEffect(resourceEffect, combatResult.Change, DateTime.Now.AddSeconds(SingleResourceChangeEffectDisplaySeconds));
        }

        public void ApplyTemporaryMaxActionToResource(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffectType resourceEffect, Vector3? position)
        {
            var expiry = DateTime.Now.AddSeconds(itemUsed.GetEffectDuration());

            var combatResult = _combatService.GetCombatResult(sourceFighter, itemUsed, resourceEffect, this);
            TriggerResourceValueUpdate(resourceEffect.ResourceTypeIdString, combatResult.Change);
            AddOrUpdateEffect(resourceEffect, combatResult.Change, expiry);
        }

        public List<ActiveEffect> GetActiveEffects()
        {
            if (AliveState != LivingEntityState.Alive)
            {
                return new List<ActiveEffect>();
            }

            return _activeEffects;
        }

        protected int GetAttributeAdjustment(AttributeAffected attributeAffected)
        {
            return _activeEffects
                .Where(x =>
                    x.Effect is IAttributeEffect attributeEffect
                    && attributeEffect.AttributeAffectedToAffect == attributeAffected)
                .Sum(x => x.Change * (x.Effect is IAttributeEffect attributeEffect && attributeEffect.IsTemporaryMaxIncrease ? 1 : -1));
        }

        private int GetResourceMaxAdjustment(string resourceTypeId)
        {
            return _activeEffects
                .Where(x =>
                    x.Effect is IResourceEffectType resourceEffect
                    && resourceEffect.ResourceTypeIdString == resourceTypeId
                    && resourceEffect.EffectActionType is EffectActionType.TemporaryMaxIncrease or EffectActionType.TemporaryMaxDecrease)
                .Sum(x => x.Change);
        }

        private bool DoesActionTypeAllowMultiple(EffectActionType effectActionType)
        {
            return effectActionType == EffectActionType.TemporaryMaxIncrease
                || effectActionType == EffectActionType.TemporaryMaxDecrease;
        }

        private void AddOrUpdateEffect(IEffectType effect, int change, DateTime expiry)
        {
            var resourceEffect = effect as IResourceEffectType;

            //todo: zzz v0.7 - Add health single decrease effect to a UI options list that can be changed
            var hideEffectFromFighter = resourceEffect != null
                && resourceEffect.EffectActionType == EffectActionType.SingleDecrease
                && resourceEffect.ResourceTypeIdString == ResourceTypeIds.HealthId;

            if (hideEffectFromFighter)
            {
                return;
            }

            var showExpiry = !(resourceEffect != null
                && resourceEffect.EffectActionType is EffectActionType.SingleDecrease or EffectActionType.SingleIncrease);

            var effectMatch = _activeEffects.FirstOrDefault(x => x.Effect == effect);

            if (effectMatch != null)
            {
                var multipleAllowed =
                    (resourceEffect != null && DoesActionTypeAllowMultiple(resourceEffect.EffectActionType))
                    || effect is IAttributeEffect;

                if (multipleAllowed)
                {
                    _activeEffects.Add(new ActiveEffect
                    {
                        Id = Guid.NewGuid(),
                        Effect = effect,
                        Change = change,
                        Expiry = expiry,
                        ShowExpiry = showExpiry
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
                    Expiry = expiry,
                    ShowExpiry = showExpiry
                });
            }

            if (OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                AddOrUpdateEffectClientRpc(effect.TypeId.ToString(), change, expiry, _rpcService.ForPlayer(OwnerClientId));
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

        #endregion
    }
}
