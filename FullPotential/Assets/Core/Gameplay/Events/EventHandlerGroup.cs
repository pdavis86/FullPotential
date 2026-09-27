using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup<TEvent> : IEventHandlerGroup where TEvent : IEvent
    {
        public HashSet<IEventHandler<TEvent>> Handlers { get; } = new HashSet<IEventHandler<TEvent>>();

        public void Add(object handler)
        {
            Handlers.Add((IEventHandler<TEvent>)handler);
        }

        public IEnumerable<T> GetHandlersOfType<T>()
        {
            return Handlers
                .Where(h => typeof(T).IsAssignableFrom(h.GetType()))
                .Select(h => (T)h);
        }

        public bool Remove(object handler)
        {
            return Handlers.Remove((IEventHandler<TEvent>)handler);
        }
    }
}
