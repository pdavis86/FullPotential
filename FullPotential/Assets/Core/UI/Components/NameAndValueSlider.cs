using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;

using UnityEngine;
using UnityEngine.UI;

namespace FullPotential.Core.UI.Components
{

    public class NameAndValueSlider : MonoBehaviour
    {
        public string Id;
        public Text Name;
        public Slider Slider;

#pragma warning disable 0649
        [SerializeField] private Text _value;
#pragma warning restore 0649

        private ILocalizer _localizer;

        private void Awake()
        {
            Slider.onValueChanged.AddListener(SliderOnValueChanged);

            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        private void SliderOnValueChanged(float newValue)
        {
            _value.text = newValue.ToString(_localizer.CurrentCulture);
        }
    }
}
