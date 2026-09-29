using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup<TEvent> : IGeneralEventHandlerGroup
        where TEvent : IEvent
    {
        public List<IEventHandler<TEvent>> Handlers { get; } = new List<IEventHandler<TEvent>>();

        internal IEventHandler<TEvent>[] HandlersSnapshot { get; private set; } = System.Array.Empty<IEventHandler<TEvent>>();

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
            RefreshSnapshot();
        }

        public bool Remove(object handler)
        {
            var typedHandler = (IEventHandler<TEvent>)handler;
            if (!Handlers.Remove(typedHandler))
            {
                return false;
            }

            RefreshSnapshot();
            return true;
        }

        private void RefreshSnapshot()
        {
            var snapshot = new IEventHandler<TEvent>[Handlers.Count];
            Handlers.CopyTo(snapshot);
            HandlersSnapshot = snapshot;
        }
    }
}
