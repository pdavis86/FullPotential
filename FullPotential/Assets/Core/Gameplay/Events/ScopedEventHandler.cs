using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public readonly struct ScopedEventHandler<TEvent> : IEquatable<ScopedEventHandler<TEvent>>
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
            return FilterFunction == null || FilterFunction(eventArgs);
        }

        public UniTask HandleEventAsync(TEvent eventArgs)
        {
            return HandlerFunction(eventArgs);
        }

        public bool Equals(ScopedEventHandler<TEvent> other)
        {
            return EqualityComparer<object>.Default.Equals(Owner, other.Owner)
                   && EqualityComparer<Func<TEvent, bool>>.Default.Equals(FilterFunction, other.FilterFunction)
                   && EqualityComparer<Func<TEvent, UniTask>>.Default.Equals(HandlerFunction, other.HandlerFunction);
        }

        public override bool Equals(object obj)
        {
            return obj is ScopedEventHandler<TEvent> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Owner, FilterFunction, HandlerFunction);
        }
    }
}
