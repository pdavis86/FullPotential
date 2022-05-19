using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry.Base;
using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IDamageable
    {
        LivingEntityState AliveState { get; }

        int GetHealthMax();

        int GetHealth();

        void TakeDamage(
            GameObject source,
            ItemBase itemUsed,
            Vector3? position);

        void HandleDeath(string killerName, string itemName);
    }
}
