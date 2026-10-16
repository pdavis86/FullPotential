using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public readonly struct AliveStateChangeEvent : IEvent
    {
        public LivingEntityBase LivingEntity { get; }

        public bool IsAlive { get; }

        public bool IsRespawning { get; }

        public AliveStateChangeEvent(LivingEntityBase livingEntity, bool isAlive, bool isRespawning = false)
        {
            LivingEntity = livingEntity;
            IsAlive = isAlive;
            IsRespawning = isRespawning;
        }
    }
}
