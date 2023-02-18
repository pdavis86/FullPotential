using FullPotential.Api.Items.Base;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IEffectService
    {
        void ApplyEffects(
            IFighter sourceFighter,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position
        );

        int GetDamageValueFromAttack(ItemBase itemUsed, int targetDefense, bool addVariation = true);
    }
}
