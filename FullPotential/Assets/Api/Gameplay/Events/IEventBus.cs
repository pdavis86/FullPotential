using System;

using Cysharp.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventBus
    {
        void Subscribe(Type handlerType);

        void Subscribe<TEvent>(Action<TEvent> handlerAction)
            where TEvent : IEvent;

        UniTask PublishAsync<TEvent>(TEvent args)
            where TEvent : IEvent;
    }
}
