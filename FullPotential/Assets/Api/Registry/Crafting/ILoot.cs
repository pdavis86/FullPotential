using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Crafting
{
    public interface ILoot : IRegisterable
    {
        ResourceConsumptionType ResourceConsumptionType { get; }
    }
}
