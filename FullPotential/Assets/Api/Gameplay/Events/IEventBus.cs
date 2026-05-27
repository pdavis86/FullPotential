using System;

using Cysharp.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventBus
    {
        void Subscribe<THandler, TArgs>(string eventId)
            where THandler : IEventHandler<TArgs>
            where TArgs : IEventHandlerArgs;

        void Subscribe(Type handlerType, string eventId);

        UniTask PublishAsync<TArgs>(string eventId, TArgs args)
            where TArgs : IEventHandlerArgs;
    }
}
