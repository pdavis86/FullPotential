using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

// Resharper disable UnusedMember.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class BasicEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        public NetworkLocation Location { get; private set; }

        public Timing Timing { get; private set; }

        public Func<TEvent, UniTask> HandlerAsync { get; private set; }

        public BasicEventHandler(
            Func<TEvent, UniTask> basicFunction,
            NetworkLocation location = NetworkLocation.Both,
            Timing timing = Timing.Main)
        {
            Location = location;
            Timing = timing;
            HandlerAsync = basicFunction;
        }

        public BasicEventHandler(
            Action<TEvent> basicAction,
            NetworkLocation location = NetworkLocation.Both,
            Timing timing = Timing.Main)
        {
            Location = location;
            Timing = timing;
            HandlerAsync = args =>
            {
                basicAction(args);
                return UniTask.CompletedTask;
            };
        }
    }
}
