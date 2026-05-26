using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup
    {
        public string EventId { get; }

        public Func<IEventHandlerArgs, UniTask> DefaultHandlerAsync { get; }

        public HashSet<IEventHandler> OtherHandlers { get; } = new HashSet<IEventHandler>();

        public EventHandlerGroup(string eventId, Func<IEventHandlerArgs, UniTask> defaultHandlerAsync)
        {
            EventId = eventId;
            DefaultHandlerAsync = defaultHandlerAsync;
        }
    }
}
