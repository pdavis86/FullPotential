using System;

using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        NetworkLocation Location { get; }

        Timing Timing { get; }

        Func<TEvent, UniTask> HandlerAsync { get; }
    }
}
