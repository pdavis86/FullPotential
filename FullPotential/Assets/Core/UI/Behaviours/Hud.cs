using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Ui.Components;
using FullPotential.Core.UI.Behaviours;
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
        private readonly List<string> _hiddenSliders = new List<string>();

        private ILocalizer _localizer;
        private ITypeRegistry _typeRegistry;

        private string _reloadingTranslation;

        private GameObject _activeEffectPrefab;
        private Image _equippedLeftHandBackground;
        private EquippedSummary _equippedLeftHandSummary;
        private Text _equippedLeftHandAmmo;
        private Image _equippedRightHandBackground;
        private EquippedSummary _equippedRightHandSummary;
        private Text _equippedRightHandAmmo;
        private FighterBase _playerFighter;
        private IEnumerable<IResource> _resources;

        #region Unity Events Handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

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
        private void FixedUpdate()
        {
            if (_playerFighter == null)
            {
                return;
            }

            //todo: zzz v0.6 - use events instead of firing on every update!
            UpdateResourceBars();
            UpdateHandOverlays();
            UpdateActiveEffects();
        }

        #endregion

        public void Initialise(FighterBase fighter)
        {
            _playerFighter = fighter;
        }

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

        public (float percent, string text) GetSliderBarValues(float currentValue, float maxValue, string extra)
        {
            return (currentValue / maxValue, $"{currentValue}/{maxValue}" + extra);
        }

        public void AddSliderBar(string id, Color color)
        {
            if (_progressBars.ContainsKey(id))
            {
                return;
            }

            var newBar = Instantiate(_resourceBarPrefab, _resourceBarsContainer.transform);
            newBar.FindInDescendants("Fill").GetComponent<Image>().color = color;

            _progressBars.Add(id, newBar);
        }

        public void UpdateSliderBar(string id, string text, float value, float maxValue)
        {
            var slider = _progressBars[id].GetComponent<BarSlider>();

            slider.UpdateValues(text, value, maxValue);

            slider.gameObject.SetActive(!_hiddenSliders.Contains(id));
        }

        public void ToggleSliderBar(string id, bool show)
        {
            if (show)
            {
                _hiddenSliders.Remove(id);
            }
            else if (!_hiddenSliders.Contains(id))
            {
                _hiddenSliders.Add(id);
            }
        }

        public void AddHandIcon(string id, bool isLeftHand, GameObject prefab)
        {
            if (_handIcons.ContainsKey(id))
            {
                return;
            }

            var container = isLeftHand ? _handIconContainerLeft : _handIconContainerRight;
            var newIcon = Instantiate(prefab, container.transform);

            _handIcons.Add(id, newIcon);
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

        private void UpdateHandOverlays()
        {
            var leftItem = _playerFighter.Inventory.GetItemInSlot(HandSlotIds.LeftHand);
            UpdateHandDescription(_equippedLeftHandSummary, leftItem);
            UpdateHandAmmo(_playerFighter.HandStatusLeft, leftItem, true);
            UpdateHandCharge(_chargeLeft, leftItem);

            var rightItem = _playerFighter.Inventory.GetItemInSlot(HandSlotIds.RightHand);
            UpdateHandDescription(_equippedRightHandSummary, rightItem);
            UpdateHandAmmo(_playerFighter.HandStatusRight, rightItem, false);
            UpdateHandCharge(_chargeRight, rightItem);
        }

        private void UpdateHandDescription(EquippedSummary equippedSummary, ItemBase item)
        {
            equippedSummary.SetContents(item?.GetDescription(_localizer));
        }

        private void UpdateHandAmmo(HandStatus handStatus, ItemBase item, bool isLeftHand)
        {
            var ammoText = isLeftHand ? _ammoLeft : _ammoRight;

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

            ammoText.text = handStatus.IsBusy
                ? _reloadingTranslation
                : $"{weapon.Ammo}/{weapon.GetAmmoMax()} ({_playerFighter.GetAvailableAmmo(isLeftHand)})";
        }

        private void UpdateHandCharge(ProgressWheel chargeWheel, ItemBase item)
        {
            //todo: this is a hack. Think of a better way
            var rangedWeapon = item is Weapon weapon && weapon.IsRanged;

            if (rangedWeapon || item is not IHasChargeUpOrCooldown hasChargeUpOrCooldown)
            {
                chargeWheel.gameObject.SetActive(false);
                return;
            }

            if (!chargeWheel.gameObject.activeInHierarchy)
            {
                chargeWheel.gameObject.SetActive(true);
            }

            chargeWheel.Slider.value = hasChargeUpOrCooldown.ChargePercentage / 100f;
        }

        private void UpdateActiveEffects()
        {
            var existingObjects = GetActiveEffectGameObjects();

            var activeEffects = _playerFighter.GetActiveEffects()
                .Where(e => e.Expiry > DateTime.Now)
                .ToList();

            if (activeEffects.Count == 0 && existingObjects.Count > 0)
            {
                foreach (var kvp in existingObjects)
                {
                    Destroy(kvp.Value);
                }

                existingObjects.Clear();
                return;
            }

            foreach (var activeEffect in activeEffects)
            {
                if (existingObjects.TryGetValue(activeEffect.Id, out var effectObject))
                {
                    var existingEffectScript = effectObject.GetComponent<ActiveEffectUi>();
                    existingEffectScript.UpdateEffect(activeEffect.Expiry);
                }
                else
                {
                    var activeEffectObj = Instantiate(_activeEffectPrefab, _activeEffectsContainer.transform);
                    var activeEffectScript = activeEffectObj.GetComponent<ActiveEffectUi>();
                    activeEffectScript.SetEffect(
                        activeEffect.Id,
                        GetEffectColor(activeEffect.Effect),
                        _localizer.Translate(activeEffect.Effect),
                        activeEffect.ShowExpiry,
                        activeEffect.Expiry);
                }
            }
        }

        private Color GetEffectColor(IEffect effect)
        {
            if (effect is IResourceEffect resourceEffect)
            {
                if (resourceEffect.EffectActionType == EffectActionType.SingleIncrease
                    || resourceEffect.EffectActionType == EffectActionType.PeriodicIncrease
                    || resourceEffect.EffectActionType == EffectActionType.TemporaryMaxIncrease)
                {
                    return Color.green;
                }

                return Color.red;
            }

            return Color.yellow;
        }

        private Dictionary<Guid, GameObject> GetActiveEffectGameObjects()
        {
            var results = new Dictionary<Guid, GameObject>();
            foreach (Transform child in _activeEffectsContainer.transform)
            {
                results.Add(child.gameObject.GetComponent<ActiveEffectUi>().Id, child.gameObject);
            }
            return results;
        }

        private Color ChangeColorAlpha(Color originalColor, float alpha)
        {
            return new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        private void SetupResourceBars()
        {
            _resources = _typeRegistry.GetRegisteredTypes<IResource>();

            foreach (var resource in _resources)
            {
                AddSliderBar(resource.TypeId.ToString(), resource.Color.ToUnityColor());
            }
        }

        private void UpdateResourceBars()
        {
            foreach (var resource in _resources)
            {
                var id = resource.TypeId.ToString();

                var value = _playerFighter.GetResourceValue(id);
                var max = _playerFighter.GetResourceMax(id);

                var (percent, text) = GetSliderBarValues(value, max, null);

                UpdateSliderBar(id, text, percent, 1);
            }
        }
    }
}