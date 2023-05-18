using System;
using FullPotential.Api.Gameplay.Items;
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

        int GetDamageValueFromAttack(ItemBase itemUsed, int targetDefense, bool addVariation = true);

        void SpawnConsumerGameObjects(IFighter sourceFighter, Consumer consumer, Vector3 startPosition, Vector3 direction);

        void SpawnConsumerVisuals(
            string prefabAddress,
            Transform parentTransform,
            Consumer consumer,
            IFighter sourceFighter,
            Vector3 startPosition,
            Vector3 direction,
            Action<ConsumerVisualsBehaviour> handleVisualsBehaviour);
    }
}
