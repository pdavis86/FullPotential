using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface ICombatService
    {
        void ApplyEffects(
            IFighter sourceFighter,
            ItemForCombatBase itemUsed,
            GameObject target,
            Vector3? position,
            float effectPercentage
        );

        int GetDamageValueFromAttack(
            IFighter sourceFighter,
            int targetDefense,
            bool addVariation = true);

        int GetDamageValueFromAttack(
            ItemForCombatBase itemUsed,
            int targetDefense,
            bool addVariation = true);

        void SpawnTargetingGameObject(
            IFighter sourceFighter,
            Consumer consumer,
            Vector3 startPosition,
            Vector3 direction);

        void SpawnShapeGameObject(
            IFighter sourceFighter,
            Consumer consumer,
            GameObject target,
            Vector3 fallbackPosition,
            Vector3 lookDirection);

        float AddVariationToValue(float basicValue);
    }
}
