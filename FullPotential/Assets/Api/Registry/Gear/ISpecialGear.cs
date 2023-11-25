using System;

namespace FullPotential.Api.Registry.Gear
{
    public interface ISpecialGear : IRegisterable
    {
        Guid SlotId { get; }
    }
}
