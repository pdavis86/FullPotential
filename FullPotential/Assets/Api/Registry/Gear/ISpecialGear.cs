using System;

namespace FullPotential.Api.Registry.Gear
{
    public interface ISpecialGear : IRegisterable, IHasEventHandlers
    {
        Guid SlotId { get; }
    }
}
