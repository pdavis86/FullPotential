using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

// Resharper disable UnusedMember.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class BasicEventHandler<TEventArgs> : IEventHandler<TEventArgs>
        where TEventArgs : IEventArgs
    {
        private readonly Func<TEventArgs, UniTask<HandlerResult>> _handlerAsync;

        public NetworkLocation Location { get; private set; }

        public Timing Timing { get; private set; }

        public BasicEventHandler(
            Func<TEventArgs, UniTask<HandlerResult>> basicFunction,
            NetworkLocation location = NetworkLocation.Both,
            Timing timing = Timing.Main)
        {
            Location = location;
            Timing = timing;
            _handlerAsync = basicFunction;
        }

        public UniTask<HandlerResult> HandleEventAsync(TEventArgs eventArgs)
        {
            return _handlerAsync(eventArgs);
        }
    }
}
