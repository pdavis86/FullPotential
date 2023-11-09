namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventManager
    {
        void Subscribe(string eventId, IEventHandler handler);

        void Unsubscribe(string eventId, IEventHandler handler);

        void Trigger(string eventId, IEventHandlerArgs args);
    }
}
