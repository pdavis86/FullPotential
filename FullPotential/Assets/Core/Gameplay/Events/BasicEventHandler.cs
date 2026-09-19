using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

// Resharper disable UnusedMember.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class BasicEventHandler<TEventArgs> : IEventHandler<TEventArgs>
        where TEventArgs : IEventArgs
    {
        public NetworkLocation Location { get; private set; }

        public Timing Timing { get; private set; }

        public Func<TEventArgs, UniTask<HandlerResult>> HandlerAsync { get; private set; }

        public BasicEventHandler(
            Func<TEventArgs, UniTask<HandlerResult>> basicFunction,
            NetworkLocation location = NetworkLocation.Both,
            Timing timing = Timing.Main)
        {
            Location = location;
            Timing = timing;
            HandlerAsync = basicFunction;
        }
    }
}
