using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessoryVisuals : IVisuals
    {
        AccessoryType Type { get; }
    }
}
