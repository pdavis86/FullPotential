using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Input;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using FullPotential.Core.Player.Events;
using FullPotential.Core.Ui.Components;
using FullPotential.Core.UI.Behaviours;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Ui.Behaviours
{
    public class Hud : MonoBehaviour, IHud
    {
#pragma warning disable 0649
        [SerializeField] private GameObject _alertsContainer;
        [SerializeField] private GameObject _activeEffectsContainer;
        [SerializeField] private GameObject _alertPrefab;
        [SerializeField] private GameObject _equippedLeftHand;
        [SerializeField] private GameObject _equippedRightHand;
        [SerializeField] private GameObject _crosshairs;
        [SerializeField] private GameObject _handIconContainerLeft;
        [SerializeField] private GameObject _handIconContainerRight;
        [SerializeField] private GameObject _resourceBarsContainer;
        [SerializeField] private GameObject _resourceBarPrefab;
        [SerializeField] private Text _ammoLeft;
        [SerializeField] private Text _ammoRight;
        [SerializeField] private ProgressWheel _chargeLeft;
        [SerializeField] private ProgressWheel _chargeRight;
#pragma warning restore 0649

        private readonly Dictionary<string, GameObject> _progressBars = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> _handIcons = new Dictionary<string, GameObject>();
        private readonly Dictionary<Guid, ActiveEffectUi> _activeEffectScripts = new Dictionary<Guid, ActiveEffectUi>();

        private IAuditor _logger;
        private ILocalizer _localizer;
        private ITypeRegistry _typeRegistry;
        private IEventBus _eventBus;

        private string _reloadingTranslation;

        private PlayerFighter _playerFighter;
        private GameObject _activeEffectPrefab;
        private Image _equippedLeftHandBackground;
        private EquippedSummary _equippedLeftHandSummary;
        private Text _equippedLeftHandAmmo;
        private Image _equippedRightHandBackground;
        private EquippedSummary _equippedRightHandSummary;
        private Text _equippedRightHandAmmo;
        private IEnumerable<IResourceType> _resources;

        private EventSubscription<LocalPlayerSpawnedEvent> _localPlayerSpawnedSubscription;

        #region Unity Events Handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _logger = DependenciesContext.Dependencies.GetService<IAuditorFactory>().Create(this);
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _eventBus = DependenciesContext.Dependencies.GetService<IEventBus>();

            SubscribeToEvents();

            _reloadingTranslation = _localizer.Translate("ui.hub.reloading");

            _activeEffectPrefab = _activeEffectsContainer.GetComponent<ActiveEffectsUi>().ActiveEffectPrefab;

            _equippedLeftHandBackground = _equippedLeftHand.GetComponent<Image>();
            _equippedLeftHandSummary = _equippedLeftHand.GetComponent<EquippedSummary>();
            _equippedLeftHandAmmo = _equippedLeftHand.transform.GetChild(0).GetComponent<Text>();

            _equippedRightHandBackground = _equippedRightHand.GetComponent<Image>();
            _equippedRightHandSummary = _equippedRightHand.GetComponent<EquippedSummary>();
            _equippedRightHandAmmo = _equippedRightHand.transform.GetChild(0).GetComponent<Text>();

            SetupResourceBars();
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            UpdateActiveEffects();
        }

        private void OnDestroy()
        {
            _eventBus.Unsubscribe(_localPlayerSpawnedSubscription);
            _eventBus.UnsubscribeBehaviour(this);
        }

        #endregion

        public void ShowAlert(string alertText)
        {
            var alertCount = _alertsContainer.transform.childCount;
            if (alertCount >= 5)
            {
                Destroy(_alertsContainer.transform.GetChild(0).gameObject);
            }

            var alert = Instantiate(_alertPrefab, _alertsContainer.transform);

            alert.GetComponent<SlideOutAlert>().Text.text = alertText;
        }

        public void ToggleDrawingMode(bool isOn)
        {
            _crosshairs.SetActive(!isOn);

            var newAlpha = isOn ? 1 : 0.5f;

            _equippedLeftHandBackground.color = ChangeColorAlpha(_equippedLeftHandBackground.color, newAlpha);

            _equippedLeftHandAmmo.color = ChangeColorAlpha(_equippedLeftHandAmmo.color, newAlpha);

            _equippedRightHandBackground.color = ChangeColorAlpha(_equippedRightHandBackground.color, newAlpha);

            _equippedRightHandAmmo.color = ChangeColorAlpha(_equippedRightHandAmmo.color, newAlpha);
        }

        public void AddSliderBar(string id, Color backgroundColor, Color textColor)
        {
            if (_progressBars.ContainsKey(id))
            {
                return;
            }

            var newBar = Instantiate(_resourceBarPrefab, _resourceBarsContainer.transform);
            newBar.FindInDescendants("Fill").GetComponent<Image>().color = backgroundColor;
            newBar.FindInDescendants("BarText").GetComponent<TextMeshProUGUI>().color = textColor;

            _progressBars.Add(id, newBar);
        }

        public void UpdateSliderBar(string id, float value, float maxValue)
        {
            var slider = _progressBars[id].GetComponent<BarSlider>();

            slider.UpdateValues(value, maxValue);
        }

        public void ToggleSliderBar(string id, bool show)
        {
            if (!_progressBars.TryGetValue(id, out var slider))
            {
                _logger.Warn("Tried to toggle slider {0} but could not find it", id);
                return;
            }

            slider.GetComponent<BarSlider>().gameObject.SetActive(show);
        }

        public void AddHandIcon(string iconId, string slotId, GameObject prefab)
        {
            if (_handIcons.ContainsKey(iconId))
            {
                return;
            }

            var container = slotId == HandSlotIds.LeftHand ? _handIconContainerLeft : _handIconContainerRight;
            var newIcon = Instantiate(prefab, container.transform);

            _handIcons.Add(iconId, newIcon);
        }

        public void RemoveHandIcon(string id)
        {
            if (!_handIcons.ContainsKey(id))
            {
                return;
            }

            var icon = _handIcons[id];

            Destroy(icon);

            _handIcons.Remove(id);
        }

        private void UpdateHandDescription(EquippedSummary equippedSummary, ItemBase item)
        {
            equippedSummary.SetContents(item?.GetDescription(_localizer));
        }

        private void UpdateHandAmmo(LivingEntityBase livingEntity, Text ammoText, SlotStatus slotStatus, ItemBase item)
        {
            if (item is not Weapon weapon
                || !weapon.IsRanged)
            {
                ammoText.transform.parent.gameObject.SetActive(false);
                return;
            }

            if (!ammoText.gameObject.activeInHierarchy)
            {
                ammoText.transform.parent.gameObject.SetActive(true);
            }

            ammoText.text = slotStatus.IsBusy
                ? _reloadingTranslation
                : $"{weapon.Ammo}/{weapon.GetAmmoMax()} ({GetAvailableAmmo(livingEntity, slotStatus.SlotId)})";
        }

        private void UpdateHandCharge(ProgressWheel chargeWheel, ItemBase item)
        {
            if (item is not IHasCharge itemWithCharge || !itemWithCharge.IsChargePercentageUsed)
            {
                chargeWheel.gameObject.SetActive(false);
                return;
            }

            if (!chargeWheel.gameObject.activeInHierarchy)
            {
                chargeWheel.gameObject.SetActive(true);
            }

            chargeWheel.Slider.value = itemWithCharge.ChargePercentage / 100f;
        }

        private void UpdateActiveEffects()
        {
            var scriptsToRemove = _activeEffectScripts
                .Where(kvp => kvp.Value.GetSecondsRemaining() <= 0)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var scriptToRemove in scriptsToRemove)
            {
                _activeEffectScripts.Remove(scriptToRemove);
            }

            foreach (var (_, existingEffectScript) in _activeEffectScripts)
            {
                existingEffectScript.UpdateEffect();
            }
        }

        private Color GetEffectColor(IEffectType effect)
        {
            if (effect is IResourceEffectType resourceEffect)
            {
                if (resourceEffect.EffectActionType is EffectActionType.SingleIncrease
                    or EffectActionType.PeriodicIncrease
                    or EffectActionType.TemporaryMaxIncrease)
                {
                    return Color.green;
                }

                return Color.red;
            }

            return Color.yellow;
        }

        private Color ChangeColorAlpha(Color originalColor, float alpha)
        {
            return new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        private void SetupResourceBars()
        {
            _resources = _typeRegistry.GetRegisteredTypes<IResourceType>();

            foreach (var resource in _resources)
            {
                AddSliderBar(
                    resource.TypeId.ToString(),
                    resource.BackgroundColor.ToUnityColor(),
                    resource.TextColor.ToUnityColor());
            }
        }

        private void SubscribeToEvents()
        {
            _localPlayerSpawnedSubscription = _eventBus.Subscribe<LocalPlayerSpawnedEvent>(HandleLocalPlayerSpawnAsync);

            _eventBus.SubscribeBehaviour<ResourceValueChangeEvent>(this, e => e.LivingEntity == _playerFighter, HandleResourceValueChangeAsync);
            _eventBus.SubscribeBehaviour<SlotChangeEvent>(this, e => e.LivingEntity == _playerFighter, e => HandleAttackOrReloadAsync(e.LivingEntity, e.SlotId, true, true, true));
            _eventBus.SubscribeBehaviour<AttackReleaseInputEvent>(this, e => e.Fighter == _playerFighter, e => HandleAttackOrReloadAsync(e.Fighter, e.SlotId, false, true, false));
            _eventBus.SubscribeBehaviour<SlotBusyChangeEvent>(this, e => e.Fighter == _playerFighter, e => HandleAttackOrReloadAsync(e.Fighter, e.SlotId, false, true, false));
            _eventBus.SubscribeBehaviour<ItemChargePercentageChangeEvent>(this, e => e.Fighter == _playerFighter, e => HandleAttackOrReloadAsync(e.Fighter, e.SlotId, false, false, true));
            _eventBus.SubscribeBehaviour<ActiveEffectAddedEvent>(this, e => e.LivingEntity == _playerFighter, HandleActiveEffectAddedAsync);
            _eventBus.SubscribeBehaviour<ActiveEffectUpdatedEvent>(this, e => e.LivingEntity == _playerFighter, HandleActiveEffectUpdatedAsync);
            _eventBus.SubscribeBehaviour<EntityDiedAfterEvent>(this, e => e.LivingEntity == _playerFighter, HandleEntityDiedAsync);
        }

        private UniTask<HandlerResult> HandleLocalPlayerSpawnAsync(LocalPlayerSpawnedEvent eventArgs)
        {
            _playerFighter = eventArgs.Fighter;

            GameManager.Instance.UserInterface.Respawn.SetActive(false);
            GameManager.Instance.UserInterface.Hud.SetActive(true);

            return UniTask.FromResult(new HandlerResult());
        }

        private void UpdateResourceBars(LivingEntityBase livingEntity)
        {
            var resourcesInUse = livingEntity.Inventory.GetResourcesInUse();

            foreach (var resource in _resources)
            {
                var id = resource.TypeId.ToString();

                ToggleSliderBar(id, resourcesInUse.Contains(id));

                var value = livingEntity.GetResourceValue(id);
                var max = livingEntity.GetResourceMax(id);

                UpdateSliderBar(id, value, max);
            }
        }

        private UniTask HandleResourceValueChangeAsync(ResourceValueChangeEvent eventArgs)
        {
            UpdateSliderBar(
                eventArgs.ResourceTypeId,
                eventArgs.NewValue,
                eventArgs.MaxValue);

            return UniTask.CompletedTask;
        }

        private UniTask HandleAttackOrReloadAsync(LivingEntityBase livingEntity, string slotId, bool isSlotChange, bool isAmmoChange, bool isChargeChange)
        {
            if (livingEntity is not FighterBase fighter)
            {
                return UniTask.CompletedTask;
            }

            var item = fighter.Inventory.GetItemInSlot(slotId);

            if (isSlotChange)
            {
                var equippedHandSummary = slotId == HandSlotIds.LeftHand ? _equippedLeftHandSummary : _equippedRightHandSummary;
                UpdateHandDescription(equippedHandSummary, item);
                UpdateResourceBars(fighter);
            }

            if (slotId is not HandSlotIds.LeftHand and not HandSlotIds.RightHand)
            {
                return UniTask.CompletedTask;
            }

            if (isAmmoChange)
            {
                var ammoComponent = slotId == HandSlotIds.LeftHand ? _ammoLeft : _ammoRight;
                var slotStatus = fighter.GetSlotStatus(slotId);
                UpdateHandAmmo(fighter, ammoComponent, slotStatus, item);
            }

            if (isChargeChange)
            {
                var chargeComponent = slotId == HandSlotIds.LeftHand ? _chargeLeft : _chargeRight;
                UpdateHandCharge(chargeComponent, item);
            }

            return UniTask.CompletedTask;
        }

        public int GetAvailableAmmo(LivingEntityBase livingEntity, string slotId)
        {
            var weapon = livingEntity.Inventory.GetItemInSlot<Weapon>(slotId);
            var ammoTypeId = weapon.WeaponType.AmmunitionTypeIdString;
            return livingEntity.Inventory.GetItemStackTotal(ammoTypeId);
        }

        private UniTask HandleActiveEffectAddedAsync(ActiveEffectAddedEvent eventArgs)
        {
            var activeEffectObj = Instantiate(_activeEffectPrefab, _activeEffectsContainer.transform);
            var activeEffectScript = activeEffectObj.GetComponent<ActiveEffectUi>();
            activeEffectScript.SetEffect(
                eventArgs.ActiveEffect.Id,
                GetEffectColor(eventArgs.ActiveEffect.Effect),
                _localizer.Translate(eventArgs.ActiveEffect.Effect),
                eventArgs.ActiveEffect.ShowExpiry,
                eventArgs.ActiveEffect.Expiry);

            _activeEffectScripts[eventArgs.ActiveEffect.Id] = activeEffectScript;

            return UniTask.CompletedTask;
        }

        private UniTask HandleActiveEffectUpdatedAsync(ActiveEffectUpdatedEvent eventArgs)
        {
            if (!_activeEffectScripts.TryGetValue(eventArgs.ActiveEffect.Id, out var existingEffectScript))
            {
                return UniTask.CompletedTask;
            }

            existingEffectScript.UpdateExpiry(eventArgs.ActiveEffect.Expiry);

            return UniTask.CompletedTask;
        }

        private UniTask HandleEntityDiedAsync(EntityDiedAfterEvent eventArgs)
        {
            foreach (var (_, script) in _activeEffectScripts)
            {
                Destroy(script.gameObject);
            }

            return UniTask.CompletedTask;
        }
    }
}
