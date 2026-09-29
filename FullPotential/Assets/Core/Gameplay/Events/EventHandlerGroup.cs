using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup<TEvent> : IGeneralEventHandlerGroup
        where TEvent : IEvent
    {
        public List<IEventHandler<TEvent>> Handlers { get; } = new List<IEventHandler<TEvent>>();

        public void Add(object handler)
        {
            var typedHandler = (IEventHandler<TEvent>)handler;
            if (Handlers.Contains(typedHandler))
            {
                return;
            }

            var insertIndex = 0;
            while (insertIndex < Handlers.Count
                   && Handlers[insertIndex].Timing.CompareTo(typedHandler.Timing) <= 0)
            {
                insertIndex++;
            }

            Handlers.Insert(insertIndex, typedHandler);
        }

        public bool Remove(object handler)
        {
            var typedHandler = (IEventHandler<TEvent>)handler;
            return Handlers.Remove(typedHandler);
        }

        public IEnumerable<IEventHandler> GetHandlers()
        {
            return Handlers;
        }
    }
}
