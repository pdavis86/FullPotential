using FullPotential.Api.EventManagement;

namespace FullPotential.Api.Gameplay.Events
{
    // todo: leave events in this folde but move other classes out
    public readonly struct LocalPlayerSpawnedEvent : IEvent
    {
        public IPlayerFighter Fighter { get; }

        public LocalPlayerSpawnedEvent(IPlayerFighter fighter)
        {
            Fighter = fighter;
        }
    }
}
