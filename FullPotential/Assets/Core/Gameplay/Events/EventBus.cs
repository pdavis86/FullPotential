using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Logging;

using Unity.Netcode;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class EventBus : IEventBus
    {
        private readonly IAuditor _logger;
        private readonly Dictionary<string, IEventHandlerGroup> _subscriptions = new Dictionary<string, IEventHandlerGroup>();

        public EventBus(IAuditorFactory auditorFactory)
        {
            _logger = auditorFactory.Create(this);
        }

        public void Register(Type eventType)
        {
            var eventId = eventType.GetCustomAttribute<RegisterEventAttribute>()?.EventId;

            if (eventId == null)
            {
                _logger.Error($"The type '{eventType}' is missing the attribute '{nameof(RegisterEventAttribute)}'");
                return;
            }

            var groupType = typeof(EventHandlerGroup<>).MakeGenericType(eventType);
            var group = (IEventHandlerGroup)DependenciesContext.Dependencies.CreateInstance(groupType);

            _subscriptions.Add(eventId, group);
        }

        public void Subscribe(Type handlerType)
        {
            var interfaceImplementation = handlerType.GetInterface(typeof(IEventHandler<>).FullName);

            if (interfaceImplementation == null)
            {
                _logger.Error($"Type '{handlerType.FullName}' does not implement {typeof(IEventHandler<>).Name}");
                return;
            }

            var eventType = handlerType
                .GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .GetGenericArguments()[0];

            if (typeof(BasicEventHandler<>).MakeGenericType(eventType).IsAssignableFrom(handlerType))
            {
                return;
            }

            var eventId = eventType.GetCustomAttribute<RegisterEventAttribute>()?.EventId;

            if (eventId == null)
            {
                _logger.Error($"The type '{eventType}' is missing the attribute '{nameof(RegisterEventAttribute)}'");
                return;
            }

            var handler = DependenciesContext.Dependencies.CreateInstance(handlerType);
            Subscribe(eventId, handler);
        }

        public void Subscribe<TEvent>(Action<TEvent> handlerAction)
            where TEvent : IEvent
        {
            var eventId = typeof(TEvent).GetCustomAttribute<RegisterEventAttribute>()?.EventId;

            if (eventId == null)
            {
                _logger.Error($"The type '{typeof(TEvent)}' is missing the attribute '{nameof(RegisterEventAttribute)}'");
                return;
            }

            var handler = new BasicEventHandler<TEvent>(handlerAction);
            Subscribe(eventId, handler);
        }

        public async UniTask PublishAsync<TEvent>(TEvent eventArgs)
            where TEvent : IEvent
        {
            eventArgs.IsCancelled = false;

            var eventId = eventArgs.GetType().GetCustomAttribute<RegisterEventAttribute>()?.EventId;

            if (eventId == null)
            {
                _logger.Error($"The type '{eventArgs.GetType()}' is missing the attribute '{nameof(RegisterEventAttribute)}'");
                return;
            }

            if (!IsEventIdRegistered(eventId))
            {
                return;
            }

            var handlerGroup = (EventHandlerGroup<TEvent>)_subscriptions[eventId];
            var timings = new List<Timing> { Timing.Before, Timing.Main, Timing.After };

            foreach (var timing in timings)
            {
                foreach (var handler in handlerGroup.Handlers)
                {
                    if (ShouldHandlerRun(handler, timing))
                    {
                        await handler.HandlerAsync(eventArgs);

                        if (eventArgs.IsCancelled)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void Subscribe(string eventId, object handler)
        {
            if (!_subscriptions.ContainsKey(eventId))
            {
                _logger.Error($"Handler '{handler.GetType().FullName}' cannot subscribe to event '{eventId}' as it has not been registered");
                return;
            }

            var group = _subscriptions[eventId];
            group.Add(handler);
        }

        private bool ShouldHandlerRun<TEvent>(IEventHandler<TEvent> handler, Timing timing)
            where TEvent : IEvent
        {
            if (handler.Timing != timing)
            {
                return false;
            }

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

            _logger.Error("No event handler has been registered for event " + eventId);
            return false;
        }
    }
}
