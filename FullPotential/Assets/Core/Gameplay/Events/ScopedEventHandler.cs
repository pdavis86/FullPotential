using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public readonly struct ScopedEventHandler<TEvent> : IScopedEventHandler
        where TEvent : IEvent
    {
        public object Owner { get; }

        public Func<TEvent, bool> FilterFunction { get; }

        public Func<TEvent, UniTask> HandlerFunction { get; }

        public ScopedEventHandler(
            object owner,
            Func<TEvent, bool> filterFunction,
            Func<TEvent, UniTask> handlerFunction)
        {
            Owner = owner;
            FilterFunction = filterFunction;
            HandlerFunction = handlerFunction;
        }

        public bool IsSupposedToRun(TEvent eventArgs)
        {
            return FilterFunction(eventArgs);
        }

        public UniTask HandleEventAsync(TEvent eventArgs)
        {
            return HandlerFunction(eventArgs);
        }
    }
}
