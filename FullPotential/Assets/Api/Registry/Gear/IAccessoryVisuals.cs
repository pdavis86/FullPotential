using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessoryVisuals : IVisuals
    {
        AccessoryCategory Category { get; }
    }
}
