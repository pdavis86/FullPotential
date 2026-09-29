using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class ScopedEventHandlerGroup<TEvent> : IScopedEventHandlerGroup
        where TEvent : IEvent
    {
        public HashSet<ScopedEventHandler<TEvent>> Handlers { get; } = new HashSet<ScopedEventHandler<TEvent>>();

        public bool Add(ScopedEventHandler<TEvent> handler)
        {
            return Handlers.Add(handler);
        }

        public bool Remove(ScopedEventHandler<TEvent> handler)
        {
            return Handlers.Remove(handler);
        }

        public void RemoveByOwner(object owner)
        {
            Handlers.RemoveWhere(
                handler => ReferenceEquals(handler.Owner, owner));
        }
    }
}
