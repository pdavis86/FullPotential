using System;

using Cysharp.Threading.Tasks;

using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventBus
    {
        void Register(Type eventType);

        void Subscribe(Type handlerType);

        void SubscribeBehaviour<TEvent>(
            MonoBehaviour behaviour,
            Action<TEvent> handlerAction)
            where TEvent : IEvent;

        void SubscribeBehaviour<TEvent>(
            MonoBehaviour behaviour,
            Func<TEvent, UniTask<HandlerResult>> handlerFunction)
            where TEvent : IEvent;

        void UnsubscribeBehaviour(MonoBehaviour behaviour);

        UniTask PublishAsync<TEvent>(TEvent eventArgs)
            where TEvent : IEvent;
    }
}
