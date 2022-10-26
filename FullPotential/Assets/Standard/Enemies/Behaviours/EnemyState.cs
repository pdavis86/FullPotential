using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Ui.Components;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyState : FighterBase
    {
        #region Inspector Variables
        // ReSharper disable UnassignedField.Compiler

        [SerializeField] private GameObject _healthSliderParent;

        // ReSharper restore UnassignedField.Compiler
        #endregion

        #region Properties

        public override Transform Transform => transform;

        public override GameObject GameObject => gameObject;

        public override Transform LookTransform => transform;

        protected override IStatSlider HealthStatSlider { get; set; }

        #endregion

        #region Unity Event Handlers

        protected override void Awake()
        {
            base.Awake();

            _health.OnValueChanged += OnHealthChanged;

            _inventory = gameObject.AddComponent<EnemyInventory>();

            HealthStatSlider = _healthSliderParent.GetComponent<IStatSlider>();
        }

        protected override void Start()
        {
            base.Start();

            UpdateUiHealthAndDefenceValues();
        }

        #endregion

        #region NetworkVariable Event Handlers

        private void OnHealthChanged(int previousValue, int newValue)
        {
            var values = _gameManager.GetUserInterface().HudOverlay.GetHealthValues(GetHealth(), GetHealthMax(), GetDefenseValue());
            HealthStatSlider.SetValues(values);
        }

        #endregion

        protected override void HandleDeathAfter(string killerName, string itemName)
        {
            Destroy(gameObject);
            _gameManager.GetSceneBehaviour().HandleEnemyDeath();
        }

    }
}
