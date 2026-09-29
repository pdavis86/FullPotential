using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class ScopedEventHandlerGroup<TEvent> : IScopedEventHandlerGroup
        where TEvent : IEvent
    {
        private readonly HashSet<ScopedEventHandler<TEvent>> _handlers = new HashSet<ScopedEventHandler<TEvent>>();

        internal ScopedEventHandler<TEvent>[] HandlersSnapshot { get; private set; } = System.Array.Empty<ScopedEventHandler<TEvent>>();

        public bool Add(ScopedEventHandler<TEvent> handler)
        {
            if (!_handlers.Add(handler))
            {
                return false;
            }

            RefreshSnapshot();
            return true;
        }

        public bool Remove(ScopedEventHandler<TEvent> handler)
        {
            if (!_handlers.Remove(handler))
            {
                return false;
            }

            RefreshSnapshot();
            return true;
        }

        public void RemoveByOwner(object owner)
        {
            var removedCount = _handlers.RemoveWhere(
                handler => ReferenceEquals(handler.Owner, owner));

            if (removedCount > 0)
            {
                RefreshSnapshot();
            }
        }

        private void RefreshSnapshot()
        {
            var snapshot = new ScopedEventHandler<TEvent>[_handlers.Count];
            _handlers.CopyTo(snapshot);
            HandlersSnapshot = snapshot;
        }
    }
}
