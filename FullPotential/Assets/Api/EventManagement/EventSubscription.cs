namespace FullPotential.Api.EventManagement
{
    public readonly struct EventSubscription<TEvent>
        where TEvent : IEvent
    {
        public IEventHandler<TEvent> Handler { get; }

        public EventSubscription(IEventHandler<TEvent> handler)
        {
            Handler = handler;
        }
    }
}
