using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Crafting
{
    public interface IGearArmor : IGear
    {
        ArmorCategory Category { get; }
    }
}
