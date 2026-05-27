using System;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Events
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterEventAttribute : Attribute
    {
        public string EventId { get; }

        public RegisterEventAttribute(string eventId)
        {
            EventId = eventId;
        }
    }
}
