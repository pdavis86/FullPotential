using FullPotential.Api.Registry.Base;
using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IDamageable
    {
        void TakeDamageFromFighter(IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position);

        void HandleDeath();
    }
}
