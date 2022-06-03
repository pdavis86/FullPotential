using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FullPotential
{
    // ReSharper disable once ClassNeverInstantiated.Global

    public class NameAndValueSlider : MonoBehaviour
    {
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnassignedField.Global
        public Text Name;
        public Slider Slider;
        // ReSharper enable MemberCanBePrivate.Global
        // ReSharper enable UnassignedField.Global

#pragma warning disable CS0649
        [SerializeField] private Text _value;
#pragma warning restore CS0649

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
