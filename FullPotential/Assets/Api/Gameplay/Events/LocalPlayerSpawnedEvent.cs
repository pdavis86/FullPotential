using FullPotential.Api.EventManagement;

namespace FullPotential.Api.Gameplay.Events
{
    public readonly struct LocalPlayerSpawnedEvent : IEvent
    {
        public IPlayerFighter Fighter { get; }

        public LocalPlayerSpawnedEvent(IPlayerFighter fighter)
        {
            Fighter = fighter;
        }
    }
}
