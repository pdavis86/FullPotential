using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Modding;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Scenes;
using FullPotential.Api.Ui.Components;
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
        public const string EventIdResourceValueChanged = "20b3ff1d-e8d0-438a-873d-98124f726e38";

        private const int VelocityThreshold = 3;
        private const int ForceThreshold = 1000;

        #region Inspector Variables
#pragma warning disable 0649
        // ReSharper disable InconsistentNaming

        [SerializeField] private TextMeshProUGUI _nameTag;
        [SerializeField] protected Transform _graphicsTransform;

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
        protected IEventManager _eventManager;
        protected ISceneService _sceneService;

        protected readonly NetworkVariable<FixedString32Bytes> _entityName = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString4096Bytes> _encodedResourceValues = new NetworkVariable<FixedString4096Bytes>();

        // ReSharper restore InconsistentNaming
        #endregion

        #region Other Variables

        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private readonly List<ActiveEffect> _activeEffects = new List<ActiveEffect>();
        private readonly Dictionary<string, int> _resourceValueCache = new Dictionary<string, int>();

        private IEnumerable<IResource> _sortedResources;

        private FighterBase _fighterWhoMovedMeLast;
        private Rigidbody _rb;

        //Action-related
        private DelayedAction _replenishResources;
        private DelayedAction _consumeStamina;

        #endregion

        #region Properties

        protected abstract IStatSlider HealthStatSlider { get; set; }

        public Rigidbody RigidBody => _rb == null ? _rb = GetComponent<Rigidbody>() : _rb;

        public LivingEntityState AliveState { get; protected set; }

        public bool IsSprinting { get; set; }

        #endregion

        #region Unity Events Handlers
        // ReSharper disable UnusedMemberHierarchy.Global

        protected virtual void Awake()
        {
            _gameManager = DependenciesContext.Dependencies.GetService<IModHelper>().GetGameManager();
            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();
            _eventManager = DependenciesContext.Dependencies.GetService<IEventManager>();
            _sceneService = _gameManager.GetSceneBehaviour().GetSceneService();

            PopulateResourceList();

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

            _encodedResourceValues.OnValueChanged += HandleEncodedResourcesChange;

            UpdateNameOnUi();
        }

        protected virtual void FixedUpdate()
        {
            RemoveExpiredEffects();

            if (!IsServer)
            {
                return;
            }

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

            _encodedResourceValues.OnValueChanged -= HandleEncodedResourcesChange;

            try
            {
                base.OnDestroy();
            }
            catch
            {
                //todo: zzz v0.6 - Remove after updating NGO and see if problem remains
                //Do nothing. Work-around for the ObjectDisposedException: The Unity.Collections.NativeList`1[System.Int32] has been deallocated, it is not allowed to access it
            }
        }

        // ReSharper restore UnusedMemberHierarchy.Global
        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void AddOrUpdateEffectClientRpc(string effectTypeId, int change, DateTime expiry, ClientRpcParams clientRpcParams)
        {
            //Debug.Log("AddOrUpdateEffectClientRpc called with typeId: " + effectTypeId);

            var effect = _typeRegistry.GetEffect(effectTypeId);
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

        private void HandleNameChange(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            UpdateNameOnUi();
        }

        protected virtual void HandleEncodedResourcesChange(FixedString4096Bytes previousValue, FixedString4096Bytes newValue)
        {
            //todo: zzz v0.9 Poor network performance. This fires a LOT e.g. stamina recharging. Maybe don't send recharge updates?

            var newValues = newValue.Value.Split(";");

            var oldValues = previousValue.Value.IsNullOrWhiteSpace()
                ? new string[newValues.Length]
                : previousValue.Value.Split(";");

            for (var i = 0; i < newValues.Length; i++)
            {
                if (oldValues[i] != newValues[i])
                {
                    var typeId = _resourceValueCache.ElementAt(i).Key;

                    _resourceValueCache[typeId] = int.Parse(newValues[i]);

                    var eventArgs = new ResourceValueChangedEventArgs(this, typeId, _resourceValueCache[typeId]);
                    _eventManager.Trigger(EventIdResourceValueChanged, eventArgs);

                    if (typeId == ResourceTypeIds.HealthId)
                    {
                        UpdateUiHealthAndDefenceValues();
                    }
                }
            }
        }

        #endregion

        #region Resource Management

        protected IEnumerable<IResource> GetResources()
        {
            return _sortedResources;
        }

        private void SetupResourceReplenishing()
        {
            _replenishResources = new DelayedAction(.2f, () =>
            {
                foreach (var resource in GetResources().Where(x => x.TypeId != ResourceTypeIds.Health))
                {
                    var typeId = resource.TypeId.ToString();
                    var value = GetResourceValue(typeId);
                    if (!IsConsumingResource(typeId) && value < GetResourceMax(typeId))
                    {
                        //todo: zzz v0.5 - trait-based resource recharge
                        AdjustResourceValue(typeId, 1);
                    }
                }
            });
        }

        public void ApplyHealthChange(
            int change,
            FighterBase sourceFighter,
            ItemBase itemUsed,
            Vector3? position,
            bool isCritical)
        {
            _lastDamageSourceName = sourceFighter != null ? sourceFighter.FighterName : null;
            _lastDamageItemName = itemUsed?.Name.OrIfNullOrWhitespace(_localizer.Translate("ui.alert.attack.noitem"));

            if (sourceFighter != null)
            {
                //Debug.Log($"'{sourceFighter.FighterName}' did {change} health change to '{_entityName.Value}' using '{itemUsed?.Name}'");

                if (change < 0)
                {
                    RecordDamageDealt(change * -1, sourceFighter);
                }

                ShowHealthChangeToSourceFighter(sourceFighter, position, change, isCritical);
            }
            //else
            //{
            //    Debug.Log("No source fighter found when trying to apply health change. Did they sign out?");
            //}

            //Do this last to ensure the entity does not die before recording the cause
            AdjustResourceValue(ResourceTypeIds.HealthId, change);
        }

        private void PopulateResourceList()
        {
            _sortedResources = _typeRegistry.GetRegisteredTypes<IResource>()
                .OrderBy(x => x.TypeId);

            foreach (var resource in _sortedResources)
            {
                _resourceValueCache.Add(resource.TypeId.ToString(), 9999);
            }

            if (!IsServer)
            {
                return;
            }

            int healthIndex;
            for (healthIndex = 0; healthIndex < _resourceValueCache.Count; healthIndex++)
            {

            }

            var sb = new StringBuilder();
            for (var i = 0; i < _resourceValueCache.Count; i++)
            {
                sb.Append(_resourceValueCache.ElementAt(i).Key == ResourceTypeIds.HealthId
                    ? GetResourceMax(ResourceTypeIds.HealthId)
                    : 0);
                sb.Append(";");
            }

            _encodedResourceValues.Value = sb.ToString();
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

        protected void AdjustResourceValue(string typeId, int change)
        {
            var currentValue = GetResourceValue(typeId);
            currentValue = ClampResourceValue(typeId, currentValue);
            SetResourceValue(typeId, currentValue + change);
        }

        protected void SetResourceInitialValues(Dictionary<string, int> values)
        {
            foreach (var kvp in values)
            {
                _resourceValueCache[kvp.Key] = ClampResourceValue(kvp.Key, kvp.Value);
            }
        }

        protected void SetResourceValue(string typeId, int newValue)
        {
            newValue = ClampResourceValue(typeId, newValue);
            _resourceValueCache[typeId] = newValue;

            var newEncodeValue = string.Join(";", _resourceValueCache.Select(x => x.Value.ToString()));
            _encodedResourceValues.Value = newEncodeValue;
        }

        protected void SetResourceValuesForRespawn()
        {
            foreach (var kvp in _resourceValueCache)
            {
                SetResourceValue(kvp.Key, GetResourceMax(kvp.Key));
            }
        }

        public int GetResourceMax(string resourceTypeId)
        {
            //todo: zzz v0.5 - trait-based resource max
            return 100 + GetResourceMaxAdjustment(resourceTypeId);
        }

        protected virtual bool IsConsumingResource(string typeId)
        {
            return false;
        }

        #endregion

        #region Sprint-specific

        //todo: zzz v0.5 - how to generalise sprint-specifics

        public int GetStaminaCost()
        {
            //todo: zzz v0.5 - trait-based sprint costs
            return 10;
        }

        public float GetSprintSpeed()
        {
            //todo: zzz v0.5 - trait-based sprint speed
            return 2.5f;
        }

        protected virtual bool IsConsumingStamina()
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
                    AdjustResourceValue(ResourceTypeIds.StaminaId, -staminaCost / 2);
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
            var defence = GetDefenseValue();
            var values = _gameManager.GetUserInterface().HudOverlay.GetHealthValues(health, maxHealth, defence);
            HealthStatSlider.SetValues(values);
        }

        #endregion

        #region Behaviour-related Methods

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

            ApplyHealthChange(healthChange, _fighterWhoMovedMeLast, null, contactPoint.point, false);
        }

        #endregion

        #region Combat-related methods

        public abstract int GetDefenseValue();

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

            StopAllCoroutines();
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
                        : _localizer.TranslateWithArgs("ui.alert.attack.youkilledyourselfusing", _lastDamageItemName);
                }

                return _lastDamageItemName.IsNullOrWhiteSpace()
                    ? _localizer.TranslateWithArgs("ui.alert.attack.youwerekilledby", _lastDamageSourceName)
                    : _localizer.TranslateWithArgs("ui.alert.attack.youwerekilledbyusing", _lastDamageSourceName, _lastDamageItemName);
            }

            if (_lastDamageSourceName == victimName)
            {
                return _lastDamageItemName.IsNullOrWhiteSpace()
                    ? _localizer.TranslateWithArgs("ui.alert.attack.victimsuicide", victimName)
                    : _localizer.TranslateWithArgs("ui.alert.attack.victimsuicideusing", victimName, _lastDamageItemName);
            }

            return _lastDamageItemName.IsNullOrWhiteSpace()
                ? _localizer.TranslateWithArgs("ui.alert.attack.victimkilledby", victimName, _lastDamageSourceName)
                : _localizer.TranslateWithArgs("ui.alert.attack.victimkilledbyusing", victimName, _lastDamageSourceName, _lastDamageItemName);
        }

        private void RecordDamageDealt(int damageDealt, FighterBase sourceFighter)
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

        private void ShowHealthChangeToSourceFighter(
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

        public void AddAttributeModifier(IAttributeEffect attributeEffect, ItemForCombatBase itemUsed, float effectPercentage)
        {
            var (change, expiry) = itemUsed.GetAttributeChangeAndExpiry(attributeEffect);
            var adjustedChange = (int)(_combatService.AddVariationToValue(change) * effectPercentage);
            AddOrUpdateEffect(attributeEffect, adjustedChange, expiry);
        }

        public void ApplyPeriodicActionToResource(IResourceEffect resourceEffect, ItemForCombatBase itemUsed, FighterBase sourceFighter, float effectPercentage)
        {
            var (change, expiry, delay) = itemUsed.GetPeriodicResourceChangeExpiryAndDelay(resourceEffect);
            var adjustedChange = (int)(_combatService.AddVariationToValue(change) * effectPercentage);
            AddOrUpdateEffect(resourceEffect, adjustedChange, expiry);
            StartCoroutine(PeriodicActionToResourceCoroutine(resourceEffect.ResourceTypeId.ToString(), adjustedChange, sourceFighter, delay, expiry));
        }

        private IEnumerator PeriodicActionToResourceCoroutine(string resourceTypeId, int change, FighterBase sourceFighter, float delay, DateTime expiry)
        {
            do
            {
                ApplyEffectChangeToResource(resourceTypeId, change, sourceFighter, null, transform.position);
                yield return new WaitForSeconds(delay);

            } while (DateTime.Now < expiry);
        }

        public void ApplyValueChangeToResource(IResourceEffect resourceEffect, ItemForCombatBase itemUsed, FighterBase sourceFighter, Vector3? position, float effectPercentage)
        {
            var change = itemUsed.GetResourceChange(resourceEffect);
            var adjustedChange = (int)(_combatService.AddVariationToValue(change) * effectPercentage);

            AddOrUpdateEffect(resourceEffect, adjustedChange, DateTime.Now.AddSeconds(3));

            ApplyEffectChangeToResource(resourceEffect.ResourceTypeId.ToString(), adjustedChange, sourceFighter, itemUsed, position);
        }

        public void ApplyTemporaryMaxActionToResource(IResourceEffect resourceEffect, ItemForCombatBase itemUsed, FighterBase sourceFighter, Vector3? position, float effectPercentage)
        {
            var (change, expiry) = itemUsed.GetResourceChangeAndExpiry(resourceEffect);
            var adjustedChange = (int)(_combatService.AddVariationToValue(change) * effectPercentage);

            AddOrUpdateEffect(resourceEffect, adjustedChange, expiry);

            ApplyEffectChangeToResource(resourceEffect.ResourceTypeId.ToString(), adjustedChange, sourceFighter, itemUsed, position);
        }

        // ReSharper disable UnusedParameter.Global
        public void ApplyElementalEffect(IEffect elementalEffect, ItemForCombatBase itemUsed, FighterBase sourceFighter, Vector3? position, float effectPercentage)
        {
            //todo: zzz v0.8 - ApplyElementalEffect
            //Debug.LogWarning("Not yet implemented elemental effects");
            ApplyHealthChange(-5, sourceFighter, null, position, false);
        }
        // ReSharper restore UnusedParameter.Global

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
                .Sum(x => x.Change * (x.Effect is IAttributeEffect attributeEffect && attributeEffect.TemporaryMaxIncrease ? 1 : -1));
        }

        private int GetResourceMaxAdjustment(string resourceTypeId)
        {
            return _activeEffects
                .Where(x =>
                    x.Effect is IResourceEffect resourceEffect
                    && resourceEffect.ResourceTypeId.ToString() == resourceTypeId
                    && resourceEffect.AffectType is AffectType.TemporaryMaxIncrease or AffectType.TemporaryMaxDecrease)
                .Sum(x => x.Change);
        }

        private bool DoesAffectAllowMultiple(AffectType affectType)
        {
            return affectType == AffectType.TemporaryMaxIncrease
                || affectType == AffectType.TemporaryMaxDecrease;
        }

        private void AddOrUpdateEffect(IEffect effect, int change, DateTime expiry)
        {
            var resourceEffect = effect as IResourceEffect;

            var showExpiry = !(resourceEffect != null
                && resourceEffect.AffectType is AffectType.SingleDecrease or AffectType.SingleIncrease);

            var effectMatch = _activeEffects.FirstOrDefault(x => x.Effect == effect);

            if (effectMatch != null)
            {
                var multipleAllowed =
                    (resourceEffect != null && DoesAffectAllowMultiple(resourceEffect.AffectType))
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

        private void ApplyEffectChangeToResource(
            string resourceTypeId,
            int change,
            FighterBase sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {
            if (resourceTypeId == ResourceTypeIds.HealthId)
            {
                ApplyHealthChange(change, sourceFighter, itemUsed, position, false);
                return;
            }

            AdjustResourceValue(resourceTypeId, change);
        }
    }
}
