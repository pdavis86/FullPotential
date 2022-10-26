using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FullPotential.Core.UI.Components
{
    // ReSharper disable ClassNeverInstantiated.Global

    public class NameAndValueSlider : MonoBehaviour
    {
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnassignedField.Global
        public Text Name;
        public Slider Slider;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore UnassignedField.Global

#pragma warning disable 0649
        [SerializeField] private Text _value;
#pragma warning restore 0649

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            Slider.onValueChanged.AddListener(SliderOnValueChanged);
        }

        private void SliderOnValueChanged(float newValue)
        {
            _value.text = newValue.ToString(CultureInfo.InvariantCulture);
        }
    }
}
