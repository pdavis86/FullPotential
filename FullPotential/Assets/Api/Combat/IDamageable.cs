using FullPotential.Api.Enums;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Combat
{
    public interface IDamageable
    {
        LivingEntityState AliveState { get; }

        int GetHealthMax();

        int GetHealth();

        void TakeDamage(int amount, ulong? clientId, string attackerName, string itemName);

        void HandleDeath(string killerName, string itemName);
    }
}
