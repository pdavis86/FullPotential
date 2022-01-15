// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Behaviours
{
    public interface IDamageable
    {
        bool IsDead { get; }

        int GetHealthMax();

        int GetHealth();

        void TakeDamage(int amount, ulong? clientId, string attackerName);

        void HandleDeath(string killerName);
    }
}
