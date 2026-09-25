using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

// Resharper disable UnusedMember.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class BasicEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        private readonly Func<TEvent, UniTask<HandlerResult>> _handlerAsync;

        public NetworkLocation Location { get; private set; }

        public Timing Timing { get; private set; }

        public BasicEventHandler(
            Func<TEvent, UniTask<HandlerResult>> basicFunction,
            NetworkLocation location = NetworkLocation.Both,
            Timing timing = Timing.Main)
        {
            Location = location;
            Timing = timing;
            _handlerAsync = basicFunction;
        }

        public UniTask<HandlerResult> HandleEventAsync(TEvent eventArgs)
        {
            return _handlerAsync(eventArgs);
        }
    }
}
