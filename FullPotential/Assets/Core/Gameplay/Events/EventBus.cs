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
            var interfaceImplementation = handlerType.GetInterface(typeof(IEventHandler<>).FullName);

            if (interfaceImplementation == null)
            {
                _logger.Error($"Type '{handlerType.FullName}' does not implement {typeof(IEventHandler<>).Name}");
                return;
            }

            var argsType = handlerType
                .GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .GetGenericArguments()[0];

            if (typeof(BasicEventHandler<>).MakeGenericType(argsType).IsAssignableFrom(handlerType))
            {
                return;
            }

            var handler = DependenciesContext.Dependencies.CreateInstance(handlerType);
            Subscribe(argsType, handler);
        }

        public void Subscribe<TEventArgs>(Action<TEventArgs> handlerAction)
            where TEventArgs : IEventArgs
        {
            var handler = new BasicEventHandler<TEventArgs>(args =>
            {
                handlerAction(args);
                return UniTask.FromResult(new HandlerResult());
            });

            Subscribe(typeof(TEventArgs), handler);
        }

        public void Subscribe<TEventArgs>(Func<TEventArgs, UniTask<HandlerResult>> handlerFunction)
            where TEventArgs : IEventArgs
        {
            var handler = new BasicEventHandler<TEventArgs>(handlerFunction);
            Subscribe(typeof(TEventArgs), handler);
        }

        public async UniTask PublishAsync<TEventArgs>(TEventArgs eventArgs)
            where TEventArgs : IEventArgs
        {
            var argsType = eventArgs.GetType();

            if (!IsEventRegistered(argsType))
            {
                _logger.Error($"No event with args type '{argsType}' was registered");
                return;
            }

            _logger.Debug($"Event with args type '{argsType}' was published");

            var handlerGroup = (EventHandlerGroup<TEventArgs>)_subscriptions[argsType];
            var timings = new List<Timing> { Timing.Before, Timing.Main, Timing.After };

            foreach (var timing in timings)
            {
                foreach (var handler in handlerGroup.Handlers)
                {
                    if (ShouldHandlerRun(handler, timing))
                    {
                        var result = await handler.HandlerAsync(eventArgs);

                        if (result.NextAction == NextAction.Cancel)
                        {
                            _logger.Debug($"Handler {handler.GetType().FullName} cancelled the remaining handlers");
                            return;
                        }

                        if (result.UpdatedEventArgs != null
                            && result.UpdatedEventArgs is TEventArgs updatedEventArgs)
                        {
                            eventArgs = updatedEventArgs;
                        }
                    }
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

        private bool ShouldHandlerRun<TEventArgs>(IEventHandler<TEventArgs> handler, Timing timing)
            where TEventArgs : IEventArgs
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
