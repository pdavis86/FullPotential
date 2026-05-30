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
    public class EventBus : IEventBus
    {
        private readonly Dictionary<string, IEventHandlerGroup> _subscriptions = new Dictionary<string, IEventHandlerGroup>();

        internal void Register<TArgs>(string eventId, Func<TArgs, UniTask> defaultHandlerAsync)
            where TArgs : IEventHandlerArgs
        {
            _subscriptions.Add(eventId, new EventHandlerGroup<TArgs>(eventId, defaultHandlerAsync));
        }

        //public void Subscribe<THandler, TArgs>(string eventId)
        //    where THandler : IEventHandler<TArgs>
        //    where TArgs : IEventHandlerArgs
        //{
        //    var handler = DependenciesContext.Dependencies.CreateInstance<THandler>();
        //    Subscribe(eventId, handler);
        //}

        public void Subscribe(string eventId, Type handlerType)
        {
            var interfaceImplementation = handlerType.GetInterface(typeof(IEventHandler<>).FullName);

            if (interfaceImplementation == null)
            {
                Debug.LogError($"Type '{handlerType.FullName}' does not implement {typeof(IEventHandler<>).Name}");
                return;
            }

            //var argumentsType = interfaceImplementation.GetGenericArguments()[0];

            var handler = DependenciesContext.Dependencies.CreateInstance(handlerType);
            Subscribe(eventId, handler);
        }

        public void Subscribe<TArgs>(string eventId, Action<TArgs> handlerAction)
            where TArgs : IEventHandlerArgs
        {
            var handler = new BasicEventHandler<TArgs>(handlerAction);
            Subscribe(eventId, handler);
        }

        public async UniTask PublishAsync<TArgs>(string eventId, TArgs args)
            where TArgs : IEventHandlerArgs
        {
            if (!IsEventIdRegistered(eventId))
            {
                return;
            }

            var handlerGroup = (EventHandlerGroup<TArgs>)_subscriptions[eventId];

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

        private void Subscribe(string eventId, object handler)
        {
            if (!_subscriptions.ContainsKey(eventId))
            {
                Debug.LogError($"Handler '{handler.GetType().FullName}' cannot subscribe to event '{eventId}' as it has not been registered");
                return;
            }

            var group = (IEventHandlerGroup)_subscriptions[eventId];
            group.Add(handler);
        }

        private bool ShouldHandlerRun<TArgs>(IEventHandler<TArgs> handler)
            where TArgs : IEventHandlerArgs
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
