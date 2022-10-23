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

        public override IStatSlider HealthStatSlider { get; protected set; }

        #endregion

        #region Unity Event Handlers

        protected override void Awake()
        {
            base.Awake();

            _health.OnValueChanged += OnHealthChanged;

            //todo: don't do this on enemy. Make a new inventory parent class
            _inventory = gameObject.AddComponent<Core.PlayerBehaviours.PlayerInventory>();

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
