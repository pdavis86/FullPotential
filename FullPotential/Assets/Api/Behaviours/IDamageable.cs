namespace FullPotential.Api.Behaviours
{
    public interface IDamageable
    {
        int GetHealthMax();

        int GetHealth();

        void TakeDamage(ulong? clientId, int amount);

        void HandleDeath();
    }
}
