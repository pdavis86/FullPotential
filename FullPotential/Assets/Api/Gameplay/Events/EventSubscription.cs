namespace FullPotential.Api.Gameplay.Events
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
