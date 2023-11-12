using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup
    {
        public string EventId { get; }

        public Action<IEventHandlerArgs> DefaultHandler { get; }

        public List<IEventHandler> OtherHandlers { get; } = new List<IEventHandler>();

        public EventHandlerGroup(string eventId, Action<IEventHandlerArgs> defaultHandler)
        {
            EventId = eventId;
            DefaultHandler = defaultHandler;
        }
    }
}
