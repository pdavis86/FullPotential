using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Obsolete.Items.Base;
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

        protected IAuditor _logger;
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
        protected readonly Dictionary<string, int> _resourceValueCache = new Dictionary<string, int>();

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
            _logger = DependenciesContext.Dependencies.GetService<IAuditorFactory>().Create(this);

            _gameManager = DependenciesContext.Dependencies.GetService<IGameManager>();
            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();
            _eventBus = DependenciesContext.Dependencies.GetService<IEventBus>();
            _sceneService = _gameManager.GetSceneBehaviour().GetSceneService();

            PopulateResourceValueCache();

            _entityName.OnValueChanged += HandleNameChange;
            _eventBus.SubscribeBehaviour<ResourceValueChangeEvent>(
                this,
                e => e.LivingEntity == this,
                HandleResourceValueChangedAsync);
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

            _eventBus.UnsubscribeBehaviour(this);

            base.OnDestroy();
        }

        // ReSharper restore UnusedMemberHierarchy.Global
        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void UpdateHealthValueClientRpc(int newValue, ClientRpcParams clientRpcParams)
        {
            // todo: zzz v0.6 - Does not work for clients joining mid-battle
            UpdateResourceValue(ResourceTypeIds.HealthId, newValue, null, null);
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void AddOrUpdateEffectClientRpc(string effectTypeId, int change, DateTime expiry, ClientRpcParams clientRpcParams)
        {
            // todo: zzz v0.6 - Does not work for clients joining mid-battle
            var effect = _typeRegistry.GetRegisteredByTypeId<IEffectType>(effectTypeId);
            AddOrUpdateEffect(effect, change, expiry);
        }

        // todo: should this be the generalised RPC?
        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        protected void ShowHudAlertClientRpc(string announcement, ClientRpcParams clientRpcParams)
        {
            // todo: Use events instead of ShowHudAlertClientRpc
            if (announcement.IsNullOrWhiteSpace())
            {
                return;
            }

            _gameManager.GetUserInterface().HudOverlay.ShowAlert(announcement);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        protected void NotifyEntityDiedClientRpc(string lastDamageSourceName, string lastDamageItemName, ClientRpcParams clientRpcParams)
        {
            PublishEntityDiedEvent(lastDamageSourceName, lastDamageItemName);
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

            if (sourceFighter == null)
            {
                return;
            }

            _logger.Debug("'{0}' did {1} health change to '{2}' using '{3}'", sourceFighter.FighterName, change, _entityName.Value, itemUsed?.Name);

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

        public void TriggerResourceValueUpdate(string typeId, int oldValue, int newValue, string sourceEntityName, string sourceItemName)
        {
            TriggerResourceValueUpdate(typeId, newValue - oldValue, sourceEntityName, sourceItemName);
        }

        public void TriggerResourceValueUpdate(string typeId, int change, string sourceEntityName, string sourceItemName)
        {
            var currentValue = ClampResourceValue(typeId, GetResourceValue(typeId));
            var newValue = ClampResourceValue(typeId, currentValue + change);
            var changeEvent = new ResourceValueChangeEvent(this, typeId, newValue, change, GetResourceMax(typeId), sourceEntityName, sourceItemName);
            _eventBus.Publish(changeEvent);
        }

        public void UpdateResourceValue(string typeId, int newValue, string sourceEntityName, string sourceItemName)
        {
            newValue = ClampResourceValue(typeId, newValue);

            if (_logger.IsEnabled(AuditLevel.Debug))
            {
                var locationName = IsServer ? "Server" : "Client";
                var registeredType = _typeRegistry.GetRegisteredByTypeId<IResourceType>(typeId);
                _logger.Debug("{0}-{1}: '{2}' changed from {3} to {4} by {5} using '{6}'", locationName, OwnerClientId, _localizer.Translate(registeredType), _resourceValueCache[typeId], newValue, sourceEntityName, sourceItemName);
            }

            _resourceValueCache[typeId] = newValue;

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
                TriggerResourceValueUpdate(resourceTypeId, GetResourceValue(resourceTypeId), GetResourceMax(resourceTypeId), name, null);
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
                    TriggerResourceValueUpdate(ResourceTypeIds.StaminaId, -staminaCost / 2, name, null);
                }
            });
        }

        #endregion

        #region UI-related Methods

        public void SetName(string newName)
        {
            if (!IsServer)
            {
                _logger.Warn("Client tried to set fighter name");
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

        private UniTask HandleResourceValueChangedAsync(ResourceValueChangeEvent eventArgs)
        {
            if (eventArgs.ResourceTypeId != ResourceTypeIds.HealthId
                || eventArgs.LivingEntity != this)
            {
                return UniTask.CompletedTask;
            }

            HealthBarSlider.UpdateValues(eventArgs.NewValue, eventArgs.MaxValue);

            return UniTask.CompletedTask;
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

            _logger.Debug("{0} collided with {1} at velocity {2} with force {3} with cause {4}", name, collision.gameObject.name, collision.relativeVelocity, force, cause);

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

            TriggerResourceValueUpdate(ResourceTypeIds.HealthId, healthChange, null, cause);
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
                    // todo: zzz v0.8 - Disconnected players miss out on loot
                    continue;
                }

                var playerState = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<IPlayerFighter>();
                playerState.SpawnLootChest(lootPosition);
            }

            _damageTaken.Clear();

            PublishEntityDiedEvent(_lastDamageSourceName, _lastDamageItemName);

            var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, 0);
            NotifyEntityDiedClientRpc(_lastDamageSourceName, _lastDamageItemName, nearbyClients);

            _activeEffectsCancellationTokenSource.Cancel();
            _activeEffectsCancellationTokenSource.Dispose();
            _activeEffectsCancellationTokenSource = new CancellationTokenSource();

            _activeEffects.Clear();

            HandleDeathAfter();
        }

        protected void PublishEntityDiedEvent(string lastDamageSourceName, string lastDamageItemName)
        {
            _eventBus.Publish(new EntityDiedAfterEvent(
                    this,
                    transform.position,
                    lastDamageSourceName,
                    lastDamageItemName)
            );
        }

        protected abstract void HandleDeathAfter();

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

        public void AddAttributeModifier(IAttributeEffect attributeEffect, int change, DateTime expiry)
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
            var combatResult = _combatService.GetCombatResult(sourceFighter, itemUsed, resourceEffect, this);
            AddOrUpdateEffect(resourceEffect, combatResult.Change, expiry);

            do
            {
                ApplySingleValueChangeToResourceInternal(sourceFighter, itemUsed, resourceEffect, position, combatResult);
                await UniTask.WaitForSeconds(delay, cancellationToken: cancellationToken);

            } while (DateTime.Now < expiry);
        }

        public void ApplySingleValueChangeToResource(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffectType resourceEffect, Vector3? position)
        {
            var combatResult = _combatService.GetCombatResult(sourceFighter, itemUsed, resourceEffect, this);
            AddOrUpdateEffect(resourceEffect, combatResult.Change, DateTime.Now.AddSeconds(SingleResourceChangeEffectDisplaySeconds));

            ApplySingleValueChangeToResourceInternal(sourceFighter, itemUsed, resourceEffect, position, combatResult);
        }

        private void ApplySingleValueChangeToResourceInternal(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffectType resourceEffect, Vector3? position, CombatResult combatResult)
        {
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

            TriggerResourceValueUpdate(resourceEffect.ResourceTypeIdString, combatResult.Change, sourceFighter.name, itemUsed.Name);
        }

        public void ApplyTemporaryMaxActionToResource(FighterBase sourceFighter, CombatItemBase itemUsed, IResourceEffectType resourceEffect)
        {
            var expiry = DateTime.Now.AddSeconds(itemUsed.GetEffectDuration());

            var combatResult = _combatService.GetCombatResult(sourceFighter, itemUsed, resourceEffect, this);
            TriggerResourceValueUpdate(resourceEffect.ResourceTypeIdString, combatResult.Change, sourceFighter.name, itemUsed.Name);
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
            return effectActionType is EffectActionType.TemporaryMaxIncrease or EffectActionType.TemporaryMaxDecrease;
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
                    var anotherActiveEffect = new ActiveEffect
                    {
                        Id = Guid.NewGuid(),
                        Effect = effect,
                        Change = change,
                        Expiry = expiry,
                        ShowExpiry = showExpiry
                    };

                    _activeEffects.Add(anotherActiveEffect);

                    _eventBus.Publish(new ActiveEffectAddedEvent(this, anotherActiveEffect));
                }
                else
                {
                    effectMatch.Change = change;
                    effectMatch.Expiry = expiry;

                    _eventBus.Publish(new ActiveEffectUpdatedEvent(this, effectMatch));
                }
            }
            else
            {
                var newActiveEffect = new ActiveEffect
                {
                    Id = Guid.NewGuid(),
                    Effect = effect,
                    Change = change,
                    Expiry = expiry,
                    ShowExpiry = showExpiry
                };

                _activeEffects.Add(newActiveEffect);

                _eventBus.Publish(new ActiveEffectAddedEvent(this, newActiveEffect));
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

        public virtual int GetMaxItems()
        {
            // todo: zzz v0.8 implement GetMaxItems()
            return 30;
        }
    }
}
