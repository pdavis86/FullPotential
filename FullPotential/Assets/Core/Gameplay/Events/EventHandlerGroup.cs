using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup<TArgs> : IEventHandlerGroup where TArgs : IEventHandlerArgs
    {
        public string EventId { get; }

        public Func<TArgs, UniTask> DefaultHandlerAsync { get; }

        public HashSet<IEventHandler<TArgs>> OtherHandlers { get; } = new HashSet<IEventHandler<TArgs>>();

        public EventHandlerGroup(string eventId, Func<TArgs, UniTask> defaultHandlerAsync)
        {
            EventId = eventId;
            DefaultHandlerAsync = defaultHandlerAsync;
        }

        public void Add(object handler)
        {
            OtherHandlers.Add((IEventHandler<TArgs>)handler);
        }
    }
}
