using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Registry.Gear
{
    public interface IHasEventHandlers
    {
        Dictionary<string, IEventHandler> EventHandlers { get; }
    }
}
