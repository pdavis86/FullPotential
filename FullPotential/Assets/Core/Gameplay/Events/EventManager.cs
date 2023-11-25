using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;
using UnityEngine;

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
            if (_subscriptions[eventId].OtherHandlers.Contains(handler))
            {
                Debug.LogWarning("Subscribe has been called multiple times with the same handler");
                return;
            }

            _subscriptions[eventId].OtherHandlers.Add(handler);
        }

        public void Unsubscribe(string eventId, IEventHandler handler)
        {
            _subscriptions[eventId].OtherHandlers.Remove(handler);
        }

        public void Trigger(string eventId, IEventHandlerArgs args)
        {
            if (!IsEventIdRegistered(eventId))
            {
                return;
            }

            var handlerGroup = _subscriptions[eventId];

            args.IsDefaultHandlerCancelled = false;

            foreach (var handler in handlerGroup.OtherHandlers)
            {
                handler.BeforeHandler?.Invoke(args);
            }

            if (!args.IsDefaultHandlerCancelled)
            {
                handlerGroup.DefaultHandler?.Invoke(args);
            }
            else if (handlerGroup.DefaultHandler == null)
            {
                Debug.LogError($"Tried to cancel the default handler for event {eventId} but no handler is present");
            }
        }

        private bool IsEventIdRegistered(string eventId)
        {
            if (_subscriptions.ContainsKey(eventId))
            {
                return true;
            }

            Debug.LogError("No event handler has been registered for event " + eventId);
            return false;

        }

        public void After(string eventId, IEventHandlerArgs args)
        {
            if (!IsEventIdRegistered(eventId))
            {
                return;
            }

            var handlerGroup = _subscriptions[eventId];

            foreach (var handler in handlerGroup.OtherHandlers)
            {
                handler.AfterHandler?.Invoke(args);
            }
        }
    }
}
