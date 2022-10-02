using FullPotential.Api.Ui.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Ui.Components
{
    public class BarSlider : MonoBehaviour, IStatSlider
    {
#pragma warning disable 0649
        private Slider _slider;
        [SerializeField] private TextMeshProUGUI _displayText;
#pragma warning restore 0649

        public void SetValues((float percent, string text) values)
        {
            if (_slider == null)
            {
                _slider = GetComponent<Slider>();
            }

            _slider.value = values.percent;
            _displayText.text = values.text;
        }
    }
}
