using System;

using Cysharp.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventBus
    {
        void Register(Type eventType);

        void Subscribe(Type handlerType);

        void Subscribe<TEventArgs>(Action<TEventArgs> handlerAction)
            where TEventArgs : IEventArgs;

        void Subscribe<TEventArgs>(Func<TEventArgs, UniTask<HandlerResult>> handlerFunction)
            where TEventArgs : IEventArgs;

        UniTask PublishAsync<TEventArgs>(TEventArgs eventArgs)
            where TEventArgs : IEventArgs;
    }
}
