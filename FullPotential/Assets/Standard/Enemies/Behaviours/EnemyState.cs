using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ui.Components;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyState : FighterBase
    {
        //todo: don't use core! Add it dynamically using the NameAndHealthCanvas prefab
        #region Inspector Variables
#pragma warning disable CS0649
        [SerializeField] private Core.Ui.Components.BarSlider _healthSlider;
#pragma warning restore CS0649
        #endregion

        #region Variables

        private string _enemyName;

        #endregion

        #region Properties

        public override Transform Transform => transform;

        public override GameObject GameObject => gameObject;

        public override Transform LookTransform => transform;

        public override string FighterName => _enemyName;

        public override IStatSlider HealthStatSlider => _healthSlider;

        #endregion

        #region Unity Event Handlers

        protected override void Awake()
        {
            base.Awake();

            _health.OnValueChanged += OnHealthChanged;

            //todo: don't do this
            _inventory = gameObject.AddComponent<Core.PlayerBehaviours.PlayerInventory>();
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
            var values = _healthSlider.GetHealthValues(GetHealth(), GetHealthMax(), GetDefenseValue());
            _healthSlider.SetValues(values);
        }

        #endregion

        public void SetName(string newName)
        {
            _enemyName = newName;

            var displayName = string.IsNullOrWhiteSpace(_enemyName)
                ? "Enemy ID " + NetworkObjectId
                : _enemyName;

            gameObject.name = displayName;
            _nameTag.text = displayName;
        }

        protected override void HandleDeathAfter(string killerName, string itemName)
        {
            Destroy(gameObject);
            _gameManager.GetSceneBehaviour().HandleEnemyDeath();
        }

    }
}
