namespace FullPotential.Api.Items
{
    public interface IHasChargeUpOrCooldown
    {
        bool IsChargePercentageUsed { get; }

        int ChargePercentage { get; set; }

        float GetChargeUpTime();

        float GetCooldownTime();
    }
}
