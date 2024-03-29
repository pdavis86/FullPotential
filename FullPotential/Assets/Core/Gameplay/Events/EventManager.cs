﻿using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using Unity.Netcode;
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

        public void Subscribe<T>(string eventId)
            where T : IEventHandler
        {
            var handler = DependenciesContext.Dependencies.CreateInstance<T>();
            _subscriptions[eventId].OtherHandlers.Add(handler);
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
                if (ShouldHandlerRun(handler))
                {
                    handler.BeforeHandler?.Invoke(args);
                }
            }

            if (!args.IsDefaultHandlerCancelled)
            {
                handlerGroup.DefaultHandler?.Invoke(args);
            }
            else if (handlerGroup.DefaultHandler == null)
            {
                Debug.LogWarning($"Tried to cancel the default handler for event {eventId} but no handler is present");
            }

            foreach (var handler in handlerGroup.OtherHandlers)
            {
                if (ShouldHandlerRun(handler))
                {
                    handler.AfterHandler?.Invoke(args);
                }
            }
        }

        private bool ShouldHandlerRun(IEventHandler handler)
        {
            switch (handler.Location)
            {
                case NetworkLocation.Server:
                    return NetworkManager.Singleton.IsServer;

                case NetworkLocation.Client:
                    return NetworkManager.Singleton.IsClient;

                default:
                    return true;
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
    }
}
