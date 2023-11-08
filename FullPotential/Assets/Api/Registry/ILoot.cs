using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry
{
    public interface ILoot : IRegisterable
    {
        ResourceType? ResourceConsumptionType { get; }
    }
}
