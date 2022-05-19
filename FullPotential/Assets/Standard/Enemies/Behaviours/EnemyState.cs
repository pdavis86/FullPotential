using FullPotential.Core.Gameplay.Combat;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyState : FighterBase
    {
        #region Properties

        public readonly NetworkVariable<FixedString32Bytes> EnemyName = new NetworkVariable<FixedString32Bytes>();

        public override Transform Transform => transform;

        public override GameObject GameObject => gameObject;

        public override Transform LookTransform => transform;

        public override string FighterName => EnemyName.Value.ToString();

        #endregion
        
        #region Unity Event Handlers

         protected override void Awake()
        {
            base.Awake();

            _health.OnValueChanged += OnHealthChanged;
            EnemyName.OnValueChanged += OnNameChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SetName();
        }

        #endregion

        #region NetworkVariable Event Handlers

        private void OnHealthChanged(int previousValue, int newValue)
        {
            var values = _healthSlider.GetHealthValues(newValue, GetHealthMax(), GetDefenseValue());
            _healthSlider.SetValues(values);
        }

        private void OnNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            SetName();
        }

        #endregion
        
        private void SetName()
        {
            var variableValue = EnemyName.Value.ToString();
            var displayName = string.IsNullOrWhiteSpace(variableValue)
                ? "Enemy " + NetworkObjectId
                : variableValue;

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
