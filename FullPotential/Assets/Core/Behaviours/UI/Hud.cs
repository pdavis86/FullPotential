using FullPotential.Core.Behaviours.PlayerBehaviours;
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
        [SerializeField] private GameObject _equippedLeftHand;
        [SerializeField] private GameObject _equippedRightHand;
        [SerializeField] private HealthSlider _healthSlider;
        //[SerializeField] private Slider _manaSlider;
        //[SerializeField] private Slider _barrierSlider;
#pragma warning restore 0649

        public void ShowAlert(string alertText)
        {
            var alertCount = _alertsContainer.transform.childCount;
            if (alertCount >= 5)
            {
                Destroy(_alertsContainer.transform.GetChild(0).gameObject);
            }

            //System.DateTime.UtcNow.ToString("ss.fff") + " " + 
            var alert = Instantiate(_alertPrefab, _alertsContainer.transform);
            alert.transform.Find("Text").GetComponent<Text>().text = alertText;
        }

        public void UpdateLeftHand(string contents)
        {
            _equippedLeftHand.GetComponent<EquippedSummary>().SetContents(contents);
        }

        public void UpdateRightHand(string contents)
        {
            _equippedRightHand.GetComponent<EquippedSummary>().SetContents(contents);
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