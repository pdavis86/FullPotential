using System;

using Cysharp.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventBus
    {
        void Register(Type eventType);

        void Subscribe(Type handlerType);

        EventSubscription<TEvent> Subscribe<TEvent>(
            Func<TEvent, UniTask<HandlerResult>> handlerFunction)
            where TEvent : IEvent;

        void Unsubscribe<TEvent>(EventSubscription<TEvent> subscription)
            where TEvent : IEvent;

        void SubscribeBehaviour<TEvent>(
            object owner,
            Func<TEvent, bool> filterFunction,
            Func<TEvent, UniTask> handlerFunction)
            where TEvent : IEvent;

        void UnsubscribeBehaviour(object owner);

        UniTask PublishAsync<TEvent>(TEvent eventArgs)
            where TEvent : IEvent;
    }
}
