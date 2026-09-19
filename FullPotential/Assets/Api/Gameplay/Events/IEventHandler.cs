using System;

using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        NetworkLocation Location { get; }

        Func<TEvent, UniTask> BeforeHandlerAsync { get; }

        Func<TEvent, UniTask> AfterHandlerAsync { get; }
    }
}
