using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup<TEvent> : IEventHandlerGroup where TEvent : IEvent
    {
        public string EventId { get; }

        public Func<TEvent, UniTask> DefaultHandlerAsync { get; }

        public HashSet<IEventHandler<TEvent>> OtherHandlers { get; } = new HashSet<IEventHandler<TEvent>>();

        public EventHandlerGroup(string eventId, Func<TEvent, UniTask> defaultHandlerAsync)
        {
            EventId = eventId;
            DefaultHandlerAsync = defaultHandlerAsync;
        }

        public void Add(object handler)
        {
            OtherHandlers.Add((IEventHandler<TEvent>)handler);
        }
    }
}
