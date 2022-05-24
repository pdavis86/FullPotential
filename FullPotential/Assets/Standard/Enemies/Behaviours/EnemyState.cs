using FullPotential.Core.Gameplay.Combat;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyState : FighterBase
    {
        #region Variables

        private string _enemyName;

        #endregion

        #region Properties

        public override Transform Transform => transform;

        public override GameObject GameObject => gameObject;

        public override Transform LookTransform => transform;

        public override string FighterName => _enemyName;

        #endregion
        
        #region Unity Event Handlers

         protected override void Awake()
        {
            base.Awake();

            _health.OnValueChanged += OnHealthChanged;
        }

        #endregion

        #region NetworkVariable Event Handlers

        private void OnHealthChanged(int previousValue, int newValue)
        {
            var values = _healthSlider.GetHealthValues(newValue, GetHealthMax(), GetDefenseValue());
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

        public override void HandleDeath(string killerName, string itemName)
        {
            base.HandleDeath(killerName, itemName);

            Destroy(gameObject);

            _gameManager.GetSceneBehaviour().HandleEnemyDeath();
        }

    }
}
