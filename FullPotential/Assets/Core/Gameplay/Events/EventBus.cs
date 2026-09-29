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
        private readonly Dictionary<Type, IGeneralEventHandlerGroup> _generalSubscriptions = new Dictionary<Type, IGeneralEventHandlerGroup>();
        private readonly Dictionary<Type, IScopedEventHandlerGroup> _scopedSubscriptions = new Dictionary<Type, IScopedEventHandlerGroup>();

        public EventBus(IAuditorFactory auditorFactory)
        {
            _logger = auditorFactory.Create(this);
        }

        public void Register(Type eventArgsType)
        {
            var attribute = eventArgsType.GetCustomAttribute<RegisterEventAttribute>();

            if (attribute == null)
            {
                _logger.Error($"The type '{eventArgsType}' is missing the attribute '{nameof(RegisterEventAttribute)}'");
                return;
            }

            var groupType = typeof(EventHandlerGroup<>).MakeGenericType(eventArgsType);
            var group = (IGeneralEventHandlerGroup)DependenciesContext.Dependencies.CreateInstance(groupType);

            _generalSubscriptions.Add(eventArgsType, group);
        }

        public void Subscribe(Type handlerType)
        {
            var interfaceImplementation = handlerType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

            if (interfaceImplementation == null)
            {
                _logger.Error($"Type '{handlerType.FullName}' does not implement {typeof(IEventHandler<>).Name}");
                return;
            }

            var argsType = interfaceImplementation.GetGenericArguments()[0];
            var handler = DependenciesContext.Dependencies.CreateInstance(handlerType);
            Subscribe(argsType, handler);
        }

        public EventSubscription<TEvent> Subscribe<TEvent>(
            Func<TEvent, UniTask<HandlerResult>> handlerFunction)
            where TEvent : IEvent
        {
            var handler = new BasicEventHandler<TEvent>(
                handlerFunction,
                NetworkLocation.Client,
                Timing.Always);
            Subscribe(typeof(TEvent), handler);
            return new EventSubscription<TEvent>(handler);
        }

        public void Unsubscribe<TEvent>(EventSubscription<TEvent> subscription)
            where TEvent : IEvent
        {
            _generalSubscriptions[typeof(TEvent)].Remove(subscription.Handler);
        }

        public void SubscribeBehaviour<TEvent>(
            object owner,
            Func<TEvent, bool> filterFunction,
            Func<TEvent, UniTask> handlerFunction)
            where TEvent : IEvent
        {
            var eventType = typeof(TEvent);

            if (!_scopedSubscriptions.TryGetValue(eventType, out var handlerGroup))
            {
                handlerGroup = new ScopedEventHandlerGroup<TEvent>();
                _scopedSubscriptions.Add(eventType, handlerGroup);
            }

            var handler = new ScopedEventHandler<TEvent>(
                owner,
                filterFunction,
                handlerFunction);

            handlerGroup.Add(handler);
        }

        public void UnsubscribeBehaviour(object owner)
        {
            foreach (var handlerGroup in _scopedSubscriptions.Values)
            {
                handlerGroup.RemoveByOwner(owner);
            }
        }

        public async UniTask PublishAsync<TEvent>(TEvent eventArgs)
            where TEvent : IEvent
        {
            var argsType = typeof(TEvent);

            if (!_generalSubscriptions.TryGetValue(argsType, out var rawGeneralHandlerGroup))
            {
                _logger.Error($"No event with args type '{argsType}' was registered");
                return;
            }

            _logger.Debug($"Event with args type '{argsType}' was published");

            var isServer = NetworkManager.Singleton.IsServer;
            var isClient = NetworkManager.Singleton.IsClient;

            var generalHandlerGroup = (EventHandlerGroup<TEvent>)rawGeneralHandlerGroup;
            foreach (var handler in generalHandlerGroup.Handlers)
            {
                if (!IsSupposedToRun(handler, isServer, isClient))
                {
                    continue;
                }

                // It's too much... _logger.Debug($"Running handler {handler.GetType().FullName}");

                var result = await handler.HandleEventAsync(eventArgs);

                if (result.UpdatedEventArgs is not null and TEvent updatedEventArgs)
                {
                    _logger.Debug($"Handler {handler.GetType().FullName} updated the event arguments");
                    eventArgs = updatedEventArgs;
                }

                if (result.NextAction == NextAction.Cancel)
                {
                    _logger.Debug($"Handler {handler.GetType().FullName} cancelled the remaining handlers");
                    break;
                }
            }

            if (_scopedSubscriptions.TryGetValue(argsType, out var scopedHandlerGroup))
            {
                var scopedHandlers = new List<IScopedEventHandler>(scopedHandlerGroup.Handlers);
                var scopedTasks = new List<UniTask>(scopedHandlers.Count);

                foreach (var rawHandler in scopedHandlers)
                {
                    var handler = (ScopedEventHandler<TEvent>)rawHandler;

                    if (handler.IsSupposedToRun(eventArgs))
                    {
                        scopedTasks.Add(handler.HandleEventAsync(eventArgs));
                    }
                }

                if (scopedTasks.Count > 0)
                {
                    await UniTask.WhenAll(scopedTasks);
                }
            }
        }

        private void Subscribe(Type argsType, object handler)
        {
            if (!_generalSubscriptions.TryGetValue(argsType, out var group))
            {
                _logger.Error($"Handler '{handler.GetType().FullName}' cannot subscribe to event with arguments type '{argsType}' as it has not been registered");
                return;
            }

            group.Add(handler);
        }

        private bool IsSupposedToRun<TEvent>(IEventHandler<TEvent> handler, bool isServer, bool isClient)
            where TEvent : IEvent
        {
            return handler.Location switch
            {
                NetworkLocation.Server => isServer,
                NetworkLocation.Client => isClient,
                _ => true,
            };
        }
    }
}
