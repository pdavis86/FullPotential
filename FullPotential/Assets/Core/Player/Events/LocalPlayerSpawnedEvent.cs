using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Player.Events
{
    [RegisterEvent]
    public readonly struct LocalPlayerSpawnedEvent : IEvent
    {
        public PlayerFighter Fighter { get; }

        public LocalPlayerSpawnedEvent(PlayerFighter fighter)
        {
            Fighter = fighter;
        }
    }
}
