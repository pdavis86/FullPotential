using FullPotential.Api.Ui.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Ui.Components
{
    public class BarSlider : MonoBehaviour, IBarSlider
    {
#pragma warning disable 0649
        private Slider _slider;
        [SerializeField] private TextMeshProUGUI _displayText;
#pragma warning restore 0649

        public void UpdateValues(string text, float value)
        {
            if (_slider == null)
            {
                _slider = GetComponent<Slider>();
            }

            _displayText.text = text;
            _slider.value = value;
        }

        public void UpdateValues(string text, float value, float maxValue)
        {
            UpdateValues(text, value);
            _slider.maxValue = maxValue;
        }
    }
}
