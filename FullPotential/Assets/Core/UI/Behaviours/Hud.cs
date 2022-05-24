﻿using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Localization;
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

        private string _reloadingTranslation;

        private GameObject _activeEffectPrefab;
        private Image _equippedLeftHandBackground;
        private EquippedSummary _equippedLeftHandSummary;
        private Text _equippedLeftHandAmmo;
        private Image _equippedRightHandBackground;
        private EquippedSummary _equippedRightHandSummary;
        private Text _equippedRightHandAmmo;
        private FighterBase _fighter;

        #region Unity Events Handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _reloadingTranslation = GameManager.Instance.GetService<Localizer>().Translate("ui.hub.reloading");

            _activeEffectPrefab = _activeEffectsContainer.GetComponent<ActiveEffects>().ActiveEffectPrefab;

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
            if (_fighter == null)
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
            _fighter = fighter;
        }

        public void ShowAlert(string alertText)
        {
            var alertCount = _alertsContainer.transform.childCount;
            if (alertCount >= 5)
            {
                Destroy(_alertsContainer.transform.GetChild(0).gameObject);
            }

            var alert = Instantiate(_alertPrefab, _alertsContainer.transform);
            alert.transform.Find("Text").GetComponent<Text>().text = alertText;
        }

        private void UpdateHandOverlays()
        {
            UpdateHandDescription(_equippedLeftHandSummary, _fighter.HandStatusLeft);
            UpdateHandAmmo(_ammoLeft, _fighter.HandStatusLeft);

            UpdateHandDescription(_equippedRightHandSummary, _fighter.HandStatusRight);
            UpdateHandAmmo(_ammoRight, _fighter.HandStatusRight);
        }

        private void UpdateHandDescription(EquippedSummary equippedSummary, HandStatus handStatus)
        {
            equippedSummary.SetContents(handStatus.EquippedItemDescription);
        }

        private void UpdateHandAmmo(Text ammoText, HandStatus handStatus)
        {
            if (handStatus == null || handStatus.EquippedWeapon == null)
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
                : $"{handStatus.EquippedWeapon.Ammo}/{handStatus.EquippedWeapon.Attributes.GetAmmoMax()}";
        }

        private void UpdateStaminaPercentage()
        {
            var values = _staminaSlider.GetStaminaValues(_fighter.GetStamina(), _fighter.GetStaminaMax());
            _staminaSlider.SetValues(values);
        }

        private void UpdateHealthPercentage()
        {
            var health = _fighter.GetHealth();
            var maxHealth = _fighter.GetHealthMax();
            var defence = _fighter.GetDefenseValue();

            var values = _healthSlider.GetHealthValues(health, maxHealth, defence);
            _healthSlider.SetValues(values);
        }

        private void UpdateManaPercentage()
        {
            var values = _manaSlider.GetManaValues(_fighter.GetMana(), _fighter.GetManaMax());
            _manaSlider.SetValues(values);
        }

        private void UpdateEnergyPercentage()
        {
            var values = _energySlider.GetEnergyValues(_fighter.GetEnergy(), _fighter.GetEnergyMax());
            _energySlider.SetValues(values);
        }

        private void UpdateActiveEffects()
        {
            var existingObjects = GetActiveEffectGameObjects();

            var activeEffects = _fighter.GetActiveEffects();

            foreach (var (effect, timeToLive) in activeEffects)
            {
                var effectType = effect.GetType();

                var activeEffectObj = existingObjects.ContainsKey(effectType)
                    ? existingObjects[effectType]
                    : Instantiate(_activeEffectPrefab, _activeEffectsContainer.transform);

                var activeEffectScript = activeEffectObj.GetComponent<ActiveEffect>();
                activeEffectScript.SetEffect(effect, timeToLive);
            }
        }

        private Dictionary<Type, GameObject> GetActiveEffectGameObjects()
        {
            var results = new Dictionary<Type, GameObject>();
            foreach (Transform child in _activeEffectsContainer.transform)
            {
                results.Add(child.gameObject.GetComponent<ActiveEffect>().Effect.GetType(), child.gameObject);
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

    }
}