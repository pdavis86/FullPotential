using System;

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
