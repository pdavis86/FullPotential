namespace FullPotential.Api.Items
{
    public interface IHasChargeUpOrCooldown
    {
        int ChargePercentage { get; set; }

        float GetChargeUpTime();

        float GetCooldownTime();
    }
}
