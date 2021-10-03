using FullPotential.Assets.Api.Behaviours;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

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

    public void TakeDamage(int amount)
    {
        Health.Value -= amount;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        _healthSlider.value = (float)newValue / MaxHealth.Value;
    }

}
