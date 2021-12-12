namespace FullPotential.Api.Behaviours
{
    public interface IDamageable
    {
        int GetHealthMax();

        int GetHealth();

        void TakeDamage(int amount);
    }
}
