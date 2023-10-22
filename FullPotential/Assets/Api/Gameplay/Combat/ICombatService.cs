using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface ICombatService
    {
        void ApplyEffects(
            IFighter sourceFighter,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position
        );

        int GetDamageValueFromAttack(
            IFighter sourceFighter,
            int targetDefense,
            bool addVariation = true);

        int GetDamageValueFromAttack(
            ItemBase itemUsed,
            int targetDefense,
            bool addVariation = true);

        void SpawnTargetingGameObject(
            IFighter sourceFighter,
            Consumer consumer,
            Vector3 startPosition,
            Vector3 direction);
    }
}
