using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

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

        internal void Register(string eventId, Func<IEventHandlerArgs, UniTask> defaultHandlerAsync)
        {
            _subscriptions.Add(eventId, new EventHandlerGroup(eventId, defaultHandlerAsync));
        }

        public void Subscribe<T>(string eventId)
            where T : IEventHandler
        {
            var handler = DependenciesContext.Dependencies.CreateInstance<T>();
            _subscriptions[eventId].OtherHandlers.Add(handler);
        }

        public async UniTask TriggerAsync(string eventId, IEventHandlerArgs args)
        {
            if (!IsEventIdRegistered(eventId))
            {
                return;
            }

            var handlerGroup = _subscriptions[eventId];

            args.IsDefaultHandlerCancelled = false;

            foreach (var handler in handlerGroup.OtherHandlers)
            {
                if (ShouldHandlerRun(handler) && handler.BeforeHandlerAsync != null)
                {
                    await handler.BeforeHandlerAsync(args);
                }
            }

            if (handlerGroup.DefaultHandlerAsync != null && !args.IsDefaultHandlerCancelled)
            {
                await handlerGroup.DefaultHandlerAsync(args);
            }
            else if (handlerGroup.DefaultHandlerAsync == null && args.IsDefaultHandlerCancelled)
            {
                Debug.LogWarning($"Tried to cancel the default handler for event {eventId} but no handler is present");
            }

            foreach (var handler in handlerGroup.OtherHandlers)
            {
                if (ShouldHandlerRun(handler) && handler.AfterHandlerAsync != null)
                {
                    await handler.AfterHandlerAsync(args);
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
