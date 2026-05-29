using System;

using Cysharp.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventBus
    {
        // todo: Can Subscribe methods be simplified by storing the eventId just on the Type?

        void Subscribe<THandler, TArgs>(string eventId)
            where THandler : IEventHandler<TArgs>
            where TArgs : IEventHandlerArgs;

        void Subscribe(string eventId, Type handlerType);

        void Subscribe<TArgs>(string eventId, Action<TArgs> handlerAction)
            where TArgs : IEventHandlerArgs;

        UniTask PublishAsync<TArgs>(string eventId, TArgs args)
            where TArgs : IEventHandlerArgs;
    }
}
