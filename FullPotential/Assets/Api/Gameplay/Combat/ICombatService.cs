using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Combat
{
    public interface ICombatService
    {
        void ApplyEffects(
            FighterBase sourceFighter,
            CombatItemBase itemUsed,
            GameObject target,
            Vector3? position,
            float effectPercentage
        );

        void SpawnTargetingGameObject(
            FighterBase sourceFighter,
            Consumer consumer,
            Vector3 startPosition,
            Vector3 direction);

        void SpawnShapeGameObject(
            FighterBase sourceFighter,
            Consumer consumer,
            GameObject target,
            Vector3 fallbackPosition,
            Vector3 lookDirection);
    }
}
