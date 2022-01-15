using FullPotential.Core.Behaviours.UI.Components;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.Ui
{
    public class Hud : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject _alertsContainer;
        [SerializeField] private GameObject _alertPrefab;
        [SerializeField] private HealthSlider _healthSlider;
        //[SerializeField] private Slider _manaSlider;
        //[SerializeField] private Slider _barrierSlider;
#pragma warning restore 0649

        public void ShowAlert(string alertText)
        {
            var alert = Instantiate(_alertPrefab, _alertsContainer.transform);
            alert.transform.Find("Text").GetComponent<Text>().text = alertText;
        }

        public void UpdateHealthPercentage(int health, int maxHealth, int defence)
        {
            _healthSlider.SetValue(health, maxHealth, defence);
        }

        //public void UpdateManaPercentage(float value)
        //{
        //    _manaSlider.value = value;
        //}

        //public void UpdateBarrierPercentage(float value)
        //{
        //    _barrierSlider.value = value;
        //}

    }
}