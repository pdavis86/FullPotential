using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Ui.Components;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyFighter : FighterBase
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

        protected override IBarSlider HealthBarSlider { get; set; }

        #endregion

        #region Unity Event Handlers

        protected override void Awake()
        {
            base.Awake();

            _inventory = gameObject.AddComponent<EnemyInventory>();

            HealthBarSlider = _healthSliderParent.GetComponent<IBarSlider>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            TriggerResourceValueUpdate(ResourceTypeIds.HealthId, 0, 100);
            UpdateUiHealthAndDefenceValues();
        }

        #endregion

        protected override void HandleDeathAfter()
        {
            Destroy(gameObject);
            _gameManager.GetSceneBehaviour().HandleEnemyDeath();
        }
    }
}
