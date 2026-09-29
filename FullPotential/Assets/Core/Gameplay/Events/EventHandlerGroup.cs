using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup<TEvent> : IGeneralEventHandlerGroup
        where TEvent : IEvent
    {
        // todo: use sorted list?
        public HashSet<IEventHandler<TEvent>> Handlers { get; } = new HashSet<IEventHandler<TEvent>>();

        public void Add(object handler)
        {
            Handlers.Add((IEventHandler<TEvent>)handler);
        }

        public bool Remove(object handler)
        {
            return Handlers.Remove((IEventHandler<TEvent>)handler);
        }

        public IEnumerable<IEventHandler> GetHandlers()
        {
            return Handlers;
        }
    }
}
