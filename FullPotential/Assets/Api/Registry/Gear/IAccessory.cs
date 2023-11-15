using FullPotential.Api.Gameplay.Player;

namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessory : IRegisterable
    {
        AccessoryLocation Location { get; }
    }
}
