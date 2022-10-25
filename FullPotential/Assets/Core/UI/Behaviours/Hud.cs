using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;
using FullPotential.Core.GameManagement;
using FullPotential.Core.UI.Behaviours;
using FullPotential.Core.Ui.Components;
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
        [SerializeField] private BarSlider _staminaSlider;
        [SerializeField] private BarSlider _healthSlider;
        [SerializeField] private BarSlider _manaSlider;
        [SerializeField] private BarSlider _energySlider;
        [SerializeField] private Text _ammoLeft;
        [SerializeField] private Text _ammoRight;
#pragma warning restore 0649

        private ILocalizer _localizer;
        private IValueCalculator _valueCalculator;

        private string _reloadingTranslation;

        private GameObject _activeEffectPrefab;
        private Image _equippedLeftHandBackground;
        private EquippedSummary _equippedLeftHandSummary;
        private Text _equippedLeftHandAmmo;
        private Image _equippedRightHandBackground;
        private EquippedSummary _equippedRightHandSummary;
        private Text _equippedRightHandAmmo;
        private FighterBase _playerFighter;

        #region Unity Events Handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _localizer = GameManager.Instance.GetService<ILocalizer>();
            _valueCalculator = GameManager.Instance.GetService<IValueCalculator>();

            _reloadingTranslation = _localizer.Translate("ui.hub.reloading");

            _activeEffectPrefab = _activeEffectsContainer.GetComponent<ActiveEffectsUi>().ActiveEffectPrefab;

            _equippedLeftHandBackground = _equippedLeftHand.GetComponent<Image>();
            _equippedLeftHandSummary = _equippedLeftHand.GetComponent<EquippedSummary>();
            _equippedLeftHandAmmo = _equippedLeftHand.transform.GetChild(0).GetComponent<Text>();

            _equippedRightHandBackground = _equippedRightHand.GetComponent<Image>();
            _equippedRightHandSummary = _equippedRightHand.GetComponent<EquippedSummary>();
            _equippedRightHandAmmo = _equippedRightHand.transform.GetChild(0).GetComponent<Text>();
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if (_playerFighter == null)
            {
                return;
            }

            UpdateStaminaPercentage();
            UpdateHealthPercentage();
            UpdateManaPercentage();
            UpdateEnergyPercentage();
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
            
            //todo: make a method on the prefab to set the text
            alert.transform.Find("Text").GetComponent<Text>().text = alertText;
        }

        private void UpdateHandOverlays()
        {
            UpdateHandDescription(_equippedLeftHandSummary, _playerFighter.HandStatusLeft);
            UpdateHandAmmo(_ammoLeft, _playerFighter.HandStatusLeft);

            UpdateHandDescription(_equippedRightHandSummary, _playerFighter.HandStatusRight);
            UpdateHandAmmo(_ammoRight, _playerFighter.HandStatusRight);
        }

        private void UpdateHandDescription(EquippedSummary equippedSummary, HandStatus handStatus)
        {
            equippedSummary.SetContents(handStatus.EquippedItemDescription);
        }

        private void UpdateHandAmmo(Text ammoText, HandStatus handStatus)
        {
            if (handStatus == null 
                || handStatus.EquippedWeapon == null
                || ((handStatus.EquippedWeapon.RegistryType is IGearWeapon gearWeapon) && gearWeapon.Category is not IGearWeapon.WeaponCategory.Ranged))
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
                : $"{handStatus.EquippedWeapon.Ammo}/{_valueCalculator.GetAmmoMax(handStatus.EquippedWeapon.Attributes)}";
        }

        private void UpdateStaminaPercentage()
        {
            var values = GetStaminaValues(_playerFighter.GetStamina(), _playerFighter.GetStaminaMax());
            _staminaSlider.SetValues(values);
        }

        private void UpdateHealthPercentage()
        {
            var health = _playerFighter.GetHealth();
            var maxHealth = _playerFighter.GetHealthMax();
            var defence = _playerFighter.GetDefenseValue();

            var values = GetHealthValues(health, maxHealth, defence);
            _healthSlider.SetValues(values);
        }

        private void UpdateManaPercentage()
        {
            var values = GetManaValues(_playerFighter.GetMana(), _playerFighter.GetManaMax());
            _manaSlider.SetValues(values);
        }

        private void UpdateEnergyPercentage()
        {
            var values = GetEnergyValues(_playerFighter.GetEnergy(), _playerFighter.GetEnergyMax());
            _energySlider.SetValues(values);
        }

        private void UpdateActiveEffects()
        {
            var existingObjects = GetActiveEffectGameObjects();

            var activeEffects = _playerFighter.GetActiveEffects();

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
                var activeEffectObj = existingObjects.ContainsKey(activeEffect.Id)
                    ? existingObjects[activeEffect.Id]
                    : Instantiate(_activeEffectPrefab, _activeEffectsContainer.transform);

                var activeEffectScript = activeEffectObj.GetComponent<ActiveEffectUi>();
                activeEffectScript.SetEffect(
                    activeEffect.Id,
                    _localizer.GetTranslatedTypeName(activeEffect.Effect),
                    (float)(activeEffect.Expiry - DateTime.Now).TotalSeconds,
                    GetEffectColor(activeEffect.Effect));
            }
        }

        private Color GetEffectColor(IEffect effect)
        {
            if (effect is IStatEffect statEffect)
            {
                if (statEffect.Affect == Affect.SingleIncrease
                    || statEffect.Affect == Affect.PeriodicIncrease
                    || statEffect.Affect == Affect.TemporaryMaxIncrease)
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

        public void ToggleCursorCapture(bool isOn)
        {
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
            return (newStamina, $"S{stamina}");
        }

        public (float percent, string text) GetHealthValues(int health, int maxHealth, int defence)
        {
            var newHealth = (float)health / maxHealth;
            return (newHealth, $"H{health} D{defence}");
        }

        public (float percent, string text) GetManaValues(int mana, int maxMana)
        {
            var newMana = (float)mana / maxMana;
            return (newMana, $"M{mana}");
        }

        public (float percent, string text) GetEnergyValues(int energy, int maxEnergy)
        {
            var newEnergy = (float)energy / maxEnergy;
            return (newEnergy, $"E{energy}");
        }

    }
}