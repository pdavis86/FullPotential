using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

using UnityEngine;

// Resharper disable UnusedMember.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class BasicEventHandler<TEvent> : IBasicEventHandler, IEventHandler<TEvent>
        where TEvent : IEvent
    {
        private readonly Func<TEvent, UniTask<HandlerResult>> _handlerAsync;

        public MonoBehaviour Behaviour { get; private set; }

        public NetworkLocation Location { get; private set; }

        public Timing Timing { get; private set; }

        public BasicEventHandler(
            MonoBehaviour behaviour,
            Func<TEvent, UniTask<HandlerResult>> basicFunction,
            NetworkLocation location = NetworkLocation.Both,
            Timing timing = Timing.Main)
        {
            Behaviour = behaviour;
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
