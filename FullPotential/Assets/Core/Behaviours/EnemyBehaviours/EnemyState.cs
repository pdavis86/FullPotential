using FullPotential.Api.Behaviours;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace FullPotential.Core.Behaviours.EnemyBehaviours
{
    public class EnemyState : NetworkBehaviour, IDefensible, IDamageable
    {
        const int _defaultHealth = 100;

        public readonly NetworkVariable<int> MaxHealth = new NetworkVariable<int>(_defaultHealth);
        public readonly NetworkVariable<int> Health = new NetworkVariable<int>(_defaultHealth);

#pragma warning disable 0649
        [SerializeField] private Slider _healthSlider;
#pragma warning restore 0649

        private void Awake()
        {
            Health.OnValueChanged += OnHealthChanged;
        }

        public int GetDefenseValue()
        {
            //todo: implement enemy GetDefenseValue()
            return 50;
        }

        public int GetHealthMax()
        {
            return _defaultHealth;
        }

        public int GetHealth()
        {
            return Health.Value;
        }

        public void TakeDamage(int amount)
        {
            Health.Value -= amount;
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            _healthSlider.value = (float)newValue / MaxHealth.Value;
        }

    }
}
