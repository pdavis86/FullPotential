using FullPotential.Api.Registry.Base;
using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IEffectHelper
    {
        void ApplyEffects(
            GameObject source,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position
        );
    }
}
