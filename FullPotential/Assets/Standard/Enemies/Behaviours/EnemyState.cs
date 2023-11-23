using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Ui.Components;
using Unity.Netcode;
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

            _inventory = gameObject.AddComponent<EnemyInventory>();

            HealthStatSlider = _healthSliderParent.GetComponent<IStatSlider>();
        }

        protected override void Start()
        {
            base.Start();

            UpdateUiHealthAndDefenceValues();
        }

        #endregion

        protected override void HandleResourceListChange(NetworkListEvent<int> changeEvent)
        {
            base.HandleResourceListChange(changeEvent);

            var health = GetResourceValue(ResourceTypeIds.HealthId);
            var healthMax = GetResourceMax(ResourceTypeIds.HealthId);
            var values = _gameManager.GetUserInterface().HudOverlay.GetHealthValues(health, healthMax, GetDefenseValue());
            HealthStatSlider.SetValues(values);
        }

        protected override void HandleDeathAfter()
        {
            Destroy(gameObject);
            _gameManager.GetSceneBehaviour().HandleEnemyDeath();
        }
    }
}
