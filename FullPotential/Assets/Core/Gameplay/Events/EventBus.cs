using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Logging;

using Unity.Netcode;

using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class EventBus : IEventBus
    {
        private readonly IAuditor _logger;
        private readonly Dictionary<Type, IEventHandlerGroup> _subscriptions = new Dictionary<Type, IEventHandlerGroup>();

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
            var group = (IEventHandlerGroup)DependenciesContext.Dependencies.CreateInstance(groupType);

            _subscriptions.Add(eventArgsType, group);
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

        public void SubscribeBehaviour<TEvent>(
            MonoBehaviour behaviour,
            Action<TEvent> handlerAction)
            where TEvent : IEvent
        {
            SubscribeBehaviour<TEvent>(
                behaviour,
                args =>
                {
                    handlerAction(args);
                    return UniTask.FromResult(new HandlerResult());
                });
        }

        public void SubscribeBehaviour<TEvent>(
            MonoBehaviour behaviour,
            Func<TEvent, UniTask<HandlerResult>> handlerFunction)
            where TEvent : IEvent
        {
            var handler = new BasicEventHandler<TEvent>(
                behaviour,
                handlerFunction,
                NetworkLocation.Client,
                Timing.Always);
            Subscribe(typeof(TEvent), handler);
        }

        public void UnsubscribeBehaviour(MonoBehaviour behaviour)
        {
            foreach (var typeKvp in _subscriptions)
            {
                var typesToRemove = typeKvp.Value.GetHandlersOfType<IBasicEventHandler>()
                    .Where(h => h.Behaviour == behaviour)
                    .ToList();

                foreach (var typeToRemove in typesToRemove)
                {
                    typeKvp.Value.Remove(typeToRemove);
                }
            }
        }

        public async UniTask PublishAsync<TEvent>(TEvent eventArgs)
            where TEvent : IEvent
        {
            var argsType = eventArgs.GetType();

            if (!IsEventRegistered(argsType))
            {
                _logger.Error($"No event with args type '{argsType}' was registered");
                return;
            }

            _logger.Debug($"Event with args type '{argsType}' was published");

            var handlerGroup = (EventHandlerGroup<TEvent>)_subscriptions[argsType];
            var sortedHanders = handlerGroup.Handlers.OrderBy(h => h.Timing);
            var filteredHandlers = sortedHanders.Where(h => ShouldHandlerRun(h)).ToList();
            var isCancelled = false;

            foreach (var handler in filteredHandlers)
            {
                if (isCancelled)
                {
                    if (handler.Timing != Timing.Always)
                    {
                        _logger.Debug($"Not running handler {handler.GetType().FullName} as the event was cancelled");
                        continue;
                    }
                    else
                    {
                        _logger.Debug($"Running handler {handler.GetType().FullName} even though the event was cancelled");
                    }
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
                    isCancelled = true;
                    _logger.Debug($"Handler {handler.GetType().FullName} cancelled the remaining handlers");
                }
            }
        }

        private void Subscribe(Type argsType, object handler)
        {
            if (!_subscriptions.ContainsKey(argsType))
            {
                _logger.Error($"Handler '{handler.GetType().FullName}' cannot subscribe to event with arguments type '{argsType}' as it has not been registered");
                return;
            }

            var group = _subscriptions[argsType];
            group.Add(handler);
        }

        private bool ShouldHandlerRun<TEvent>(IEventHandler<TEvent> handler)
            where TEvent : IEvent
        {
            return handler.Location switch
            {
                NetworkLocation.Server => NetworkManager.Singleton.IsServer,
                NetworkLocation.Client => NetworkManager.Singleton.IsClient,
                _ => true,
            };
        }

        private bool IsEventRegistered(Type argsType)
        {
            if (_subscriptions.ContainsKey(argsType))
            {
                return true;
            }

            _logger.Error("No event handler has been registered for event with arguments type " + argsType);
            return false;
        }
    }
}
