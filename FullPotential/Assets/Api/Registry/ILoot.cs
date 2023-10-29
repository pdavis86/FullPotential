using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry
{
    public interface ILoot : IRegisterable
    {
        ResourceConsumptionType ResourceConsumptionType { get; }
    }
}
