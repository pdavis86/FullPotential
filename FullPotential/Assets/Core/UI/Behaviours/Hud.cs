using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;
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
        [SerializeField] private GameObject _handWarningLeft;
        [SerializeField] private GameObject _handWarningRight;
        [SerializeField] private GameObject _resourceBarsContainer;
        [SerializeField] private GameObject _resourceBarPrefab;
        [SerializeField] private Text _ammoLeft;
        [SerializeField] private Text _ammoRight;
        [SerializeField] private ProgressWheel _chargeLeft;
        [SerializeField] private ProgressWheel _chargeRight;
#pragma warning restore 0649

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
        private BarSlider _staminaSlider;
        private BarSlider _healthSlider;
        private BarSlider _manaSlider;
        private BarSlider _energySlider;

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
            UpdateStaminaPercentage();
            UpdateHealthPercentage();
            UpdateManaPercentage();
            UpdateEnergyPercentage();

            UpdateHandOverlays();

            UpdateActiveEffects();
        }

        #endregion

        private void SetupResourceBars()
        {
            var resources = _typeRegistry.GetRegisteredTypes<IResource>();

            foreach (var resource in resources)
            {
                var newBar = Instantiate(_resourceBarPrefab, _resourceBarsContainer.transform);
                newBar.FindInDescendants("Fill").GetComponent<Image>().color = resource.Color.ToUnityColor();

                switch (resource.TypeId.ToString())
                {
                    case ResourceTypeIds.HealthId:
                        _healthSlider = newBar.GetComponent<BarSlider>();
                        break;

                    case ResourceTypeIds.StaminaId:
                        _staminaSlider = newBar.GetComponent<BarSlider>();
                        break;

                    case ResourceTypeIds.ManaId:
                        _manaSlider = newBar.GetComponent<BarSlider>();
                        break;

                    case ResourceTypeIds.EnergyId:
                        _energySlider = newBar.GetComponent<BarSlider>();
                        break;
                }
            }
        }

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
                ammoText.gameObject.SetActive(false);
                return;
            }

            if (!ammoText.gameObject.activeInHierarchy)
            {
                ammoText.gameObject.SetActive(true);
            }

            ammoText.text = handStatus.IsReloading
                ? _reloadingTranslation
                : $"{weapon.Ammo}/{weapon.GetAmmoMax()} ({_playerFighter.GetAvailableAmmo(isLeftHand)})";
        }

        private void UpdateHandCharge(ProgressWheel chargeWheel, ItemBase item)
        {
            if (item is not Consumer consumer)
            {
                chargeWheel.gameObject.SetActive(false);
                return;
            }

            if (!chargeWheel.gameObject.activeInHierarchy)
            {
                chargeWheel.gameObject.SetActive(true);
            }

            chargeWheel.Slider.value = consumer.ChargePercentage / 100f;
        }

        private void UpdateStaminaPercentage()
        {
            var values = GetStaminaValues(_playerFighter.GetResourceValue(ResourceTypeIds.StaminaId), _playerFighter.GetResourceMax(ResourceTypeIds.StaminaId));
            _staminaSlider.SetValues(values);
        }

        private void UpdateHealthPercentage()
        {
            var health = _playerFighter.GetResourceValue(ResourceTypeIds.HealthId);
            var maxHealth = _playerFighter.GetResourceMax(ResourceTypeIds.HealthId);
            var defence = _playerFighter.GetDefenseValue();

            var values = GetHealthValues(health, maxHealth, defence);
            _healthSlider.SetValues(values);
        }

        private void UpdateManaPercentage()
        {
            var values = GetManaValues(_playerFighter.GetResourceValue(ResourceTypeIds.ManaId), _playerFighter.GetResourceMax(ResourceTypeIds.ManaId));
            _manaSlider.SetValues(values);
        }

        private void UpdateEnergyPercentage()
        {
            var values = GetEnergyValues(_playerFighter.GetResourceValue(ResourceTypeIds.EnergyId), _playerFighter.GetResourceMax(ResourceTypeIds.EnergyId));
            _energySlider.SetValues(values);
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
                if (resourceEffect.AffectType == AffectType.SingleIncrease
                    || resourceEffect.AffectType == AffectType.PeriodicIncrease
                    || resourceEffect.AffectType == AffectType.TemporaryMaxIncrease)
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

        public void ToggleDrawingMode(bool isOn)
        {
            _crosshairs.SetActive(!isOn);

            var newAlpha = isOn ? 1 : 0.5f;

            _equippedLeftHandBackground.color = ChangeColorAlpha(_equippedLeftHandBackground.color, newAlpha);

            _equippedLeftHandAmmo.color = ChangeColorAlpha(_equippedLeftHandAmmo.color, newAlpha);

            _equippedRightHandBackground.color = ChangeColorAlpha(_equippedRightHandBackground.color, newAlpha);

            _equippedRightHandAmmo.color = ChangeColorAlpha(_equippedRightHandAmmo.color, newAlpha);
        }

        private Color ChangeColorAlpha(Color originalColor, float alpha)
        {
            return new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        public (float percent, string text) GetStaminaValues(int stamina, int maxStamina)
        {
            var newStamina = (float)stamina / maxStamina;
            return (newStamina, $"{stamina}/{maxStamina}");
        }

        public (float percent, string text) GetHealthValues(int health, int maxHealth, int defence)
        {
            var newHealth = (float)health / maxHealth;
            return (newHealth, $"{health}/{maxHealth} (D{defence})");
        }

        public (float percent, string text) GetManaValues(int mana, int maxMana)
        {
            var newMana = (float)mana / maxMana;
            return (newMana, $"{mana}/{maxMana}");
        }

        public (float percent, string text) GetEnergyValues(int energy, int maxEnergy)
        {
            var newEnergy = (float)energy / maxEnergy;
            return (newEnergy, $"{energy}/{maxEnergy}");
        }

        public void SetHandWarning(bool isLeftHand, bool isActive)
        {
            var handWarningObject = isLeftHand ? _handWarningLeft : _handWarningRight;
            handWarningObject.SetActive(isActive);
        }
    }
}