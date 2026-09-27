using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    [RegisterEvent]
    public readonly struct ActiveEffectUpdatedEvent : IEvent
    {
        public LivingEntityBase LivingEntity { get; }

        public ActiveEffect ActiveEffect { get; }

        public ActiveEffectUpdatedEvent(LivingEntityBase livingEntity, ActiveEffect activeEffect)
        {
            LivingEntity = livingEntity;
            ActiveEffect = activeEffect;
        }
    }
}
