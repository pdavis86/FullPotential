using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Items;
using FullPotential.Api.Registry.Effects;

using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface ICombatService
    {
        void ApplyEffects(
            FighterBase sourceFighter,
            ICombatItem itemUsed,
            GameObject target,
            Vector3? position
        );

        void SpawnTargetingGameObject(
            FighterBase sourceFighter,
            IConsumer consumer,
            Vector3 startPosition,
            Vector3 direction);

        void SpawnShapeGameObject(
            FighterBase sourceFighter,
            IConsumer consumer,
            GameObject target,
            Vector3 fallbackPosition,
            Vector3 lookDirection);

        float GetEffectBaseChange(
            FighterBase sourceFighter,
            ICombatItem itemUsed,
            IEffectType effect,
            bool isCriticalHit);

        CombatResult GetCombatResult(
            FighterBase sourceFighter,
            ICombatItem itemUsed,
            IEffectType effect,
            LivingEntityBase targetLivingEntity);
    }
}
