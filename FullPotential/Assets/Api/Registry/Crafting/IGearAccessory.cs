using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Crafting
{
    public interface IGearAccessory : IGear
    {
        AccessoryCategory Category { get; }
    }
}
