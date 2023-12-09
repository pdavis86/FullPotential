
// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventManager
    {
        void Subscribe<T>(string eventId) where T : IEventHandler;

        void Trigger(string eventId, IEventHandlerArgs args);
    }
}
