using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public interface IScopedEventHandlerGroup : IEventHandlerGroup
    {
        HashSet<IScopedEventHandler> Handlers { get; }

        void RemoveByOwner(object owner);
    }
}
