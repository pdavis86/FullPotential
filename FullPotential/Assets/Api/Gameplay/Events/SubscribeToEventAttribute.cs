using System;

namespace FullPotential.Api.Gameplay.Events
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SubscribeToEventAttribute : Attribute
    {
        public string EventId { get; }

        public SubscribeToEventAttribute(string eventId)
        {
            EventId = eventId;
        }
    }
}
