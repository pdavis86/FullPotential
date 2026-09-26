using System;

using Cysharp.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventBus
    {
        void Register(Type eventType);

        void Subscribe(Type handlerType);

        void Subscribe<TEvent>(
            Action<TEvent> handlerAction,
            NetworkLocation location = NetworkLocation.Both,
            Timing timing = Timing.Main)
            where TEvent : IEvent;

        void Subscribe<TEvent>(
            Func<TEvent, UniTask<HandlerResult>> handlerFunction,
            NetworkLocation location = NetworkLocation.Both,
            Timing timing = Timing.Main)
            where TEvent : IEvent;

        UniTask PublishAsync<TEvent>(TEvent eventArgs)
            where TEvent : IEvent;
    }
}
