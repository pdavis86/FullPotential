using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.UI.Components
{
    public class HealthSlider : MonoBehaviour
    {
#pragma warning disable 0649
        private Slider _slider;
        [SerializeField] private TextMeshProUGUI _displayText;
#pragma warning restore 0649

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        public void SetValue(int health, int maxHealth, int defence)
        {
            var newHealth = (float)health / maxHealth;
            _slider.value = newHealth;
            _displayText.text = $"H{health} D{defence}";
        }
    }
}
