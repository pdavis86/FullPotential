using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class EventManager : IEventManager
    {
        private readonly Dictionary<string, EventHandlerGroup> _subscriptions = new Dictionary<string, EventHandlerGroup>();

        internal void Register(string eventId, Action<IEventHandlerArgs> defaultHandler)
        {
            _subscriptions.Add(eventId, new EventHandlerGroup(eventId, defaultHandler));
        }

        public void Subscribe(string eventId, IEventHandler handler)
        {
            _subscriptions[eventId].OtherHandlers.Add(handler);
        }

        public void Unsubscribe(string eventId, IEventHandler handler)
        {
            _subscriptions[eventId].OtherHandlers.Remove(handler);
        }

        public void Trigger(string eventId, IEventHandlerArgs args)
        {
            var handlerGroup = _subscriptions[eventId];

            args.IsDefaultHandlerCancelled = false;

            foreach (var handler in handlerGroup.OtherHandlers)
            {
                handler.BeforeEvent?.Invoke(args);
            }

            if (!args.IsDefaultHandlerCancelled)
            {
                handlerGroup.DefaultHandler?.Invoke(args);
            }

            foreach (var handler in handlerGroup.OtherHandlers)
            {
                handler.AfterEvent?.Invoke(args);
            }
        }
    }
}
