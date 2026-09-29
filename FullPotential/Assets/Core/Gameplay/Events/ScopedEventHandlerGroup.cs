using System.Collections;
using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class ScopedEventHandlerGroup<TEvent> : IScopedEventHandlerGroup
        where TEvent : IEvent
    {
        public HashSet<IScopedEventHandler> Handlers { get; } = new HashSet<IScopedEventHandler>();

        public IEnumerator GetEnumerator()
        {
            return Handlers.GetEnumerator();
        }

        public void Add(object handler)
        {
            Handlers.Add((IScopedEventHandler)handler);
        }

        public bool Remove(object handler)
        {
            return Handlers.Remove((IScopedEventHandler)handler);
        }

        public void RemoveByOwner(object owner)
        {
            Handlers.RemoveWhere(
                handler => handler is ScopedEventHandler<TEvent> scopedHandler
                && ReferenceEquals(scopedHandler.Owner, owner));
        }
    }
}
