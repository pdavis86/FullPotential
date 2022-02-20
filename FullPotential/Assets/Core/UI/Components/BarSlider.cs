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

        public (float percent, string text) GetStaminaValues(int stamina, int maxStamina)
        {
            var newStamina = (float)stamina / maxStamina;
            return (newStamina, $"S{stamina}");
        }

        public (float percent, string text) GetHealthValues(int health, int maxHealth, int defence)
        {
            var newHealth = (float)health / maxHealth;
            return (newHealth, $"H{health} D{defence}");
        }

        public (float percent, string text) GetManaValues(int mana, int maxMana)
        {
            var newMana = (float)mana / maxMana;
            return (newMana, $"M{mana}");
        }

        public (float percent, string text) GetEnergyValues(int energy, int maxEnergy)
        {
            var newEnergy = (float)energy / maxEnergy;
            return (newEnergy, $"E{energy}");
        }
    }
}
