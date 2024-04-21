namespace FullPotential.Api.Items
{
    public interface IHasCharge
    {
        bool IsChargePercentageUsed { get; }

        int ChargePercentage { get; set; }

        float GetChargeUpTime();

        float GetCooldownTime();
    }
}
