using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Gear
{
    public interface IArmorVisuals : IVisuals
    {
        ArmorType Type { get; }
    }
}
