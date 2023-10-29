using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Gear
{
    public interface IArmorVisuals : IVisuals
    {
        ArmorCategory Category { get; }
    }
}
