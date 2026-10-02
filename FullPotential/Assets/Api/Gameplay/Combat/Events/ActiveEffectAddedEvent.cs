using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public readonly struct ActiveEffectAddedEvent : IEvent
    {
        public LivingEntityBase LivingEntity { get; }

        public ActiveEffect ActiveEffect { get; }

        public ActiveEffectAddedEvent(LivingEntityBase livingEntity, ActiveEffect activeEffect)
        {
            LivingEntity = livingEntity;
            ActiveEffect = activeEffect;
        }
    }
}
