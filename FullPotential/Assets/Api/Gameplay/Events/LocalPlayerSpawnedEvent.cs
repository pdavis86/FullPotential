using FullPotential.Api.Gameplay;

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
