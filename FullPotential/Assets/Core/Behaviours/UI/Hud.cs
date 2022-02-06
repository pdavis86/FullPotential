using FullPotential.Api.Data;
using FullPotential.Api.Ui;
using FullPotential.Core.Behaviours.UI.Components;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.Ui
{
    public class Hud : MonoBehaviour, IHud
    {
#pragma warning disable 0649
        [SerializeField] private GameObject _alertsContainer;
        [SerializeField] private GameObject _alertPrefab;
        [SerializeField] private GameObject _equippedLeftHand;
        [SerializeField] private GameObject _equippedRightHand;
        [SerializeField] private BarSlider _staminaSlider;
        [SerializeField] private BarSlider _healthSlider;
        [SerializeField] private BarSlider _manaSlider;
        [SerializeField] private Text _ammoLeft;
        [SerializeField] private Text _ammoRight;
#pragma warning restore 0649

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

        public void UpdateHand(string contents, bool isLeftHand)
        {
            var leftOrRight = isLeftHand
                ? _equippedLeftHand
                : _equippedRightHand;

            leftOrRight.GetComponent<EquippedSummary>().SetContents(contents);
        }

        public void UpdateAmmo(bool isLeftHand, PlayerHandStatus playerHandStatus)
        {
            var leftOrRight = isLeftHand
                ? _ammoLeft
                : _ammoRight;

            if (playerHandStatus == null)
            {
                leftOrRight.gameObject.SetActive(false);
                return;
            }

            leftOrRight.gameObject.SetActive(true);
            leftOrRight.text = $"{playerHandStatus.Ammo}/{playerHandStatus.AmmoMax}";
        }

        public void UpdateStaminaPercentage(int stamina, int maxStamina)
        {
            var values = _staminaSlider.GetStaminaValues(stamina, maxStamina);
            _staminaSlider.SetValues(values);
        }

        public void UpdateHealthPercentage(int health, int maxHealth, int defence)
        {
            var values = _healthSlider.GetHealthValues(health, maxHealth, defence);
            _healthSlider.SetValues(values);
        }

        public void UpdateManaPercentage(int mana, int maxMana)
        {
            var values = _manaSlider.GetManaValues(mana, maxMana);
            _manaSlider.SetValues(values);
        }

        public void ToggleCursorCapture(bool isOn)
        {
            var newAlpha = isOn ? 1 : 0.5f;

            var leftImage = _equippedLeftHand.GetComponent<Image>();
            leftImage.color = ChangeColorAlpha(leftImage.color, newAlpha);

            var leftText = _equippedLeftHand.transform.GetChild(0).GetComponent<Text>();
            leftText.color = ChangeColorAlpha(leftText.color, newAlpha);

            var rightImage = _equippedRightHand.GetComponent<Image>();
            rightImage.color = ChangeColorAlpha(leftImage.color, newAlpha);

            var rightText = _equippedRightHand.transform.GetChild(0).GetComponent<Text>();
            rightText.color = ChangeColorAlpha(leftText.color, newAlpha);
        }

        private Color ChangeColorAlpha(Color originalColor, float alpha)
        {
            return new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

    }
}