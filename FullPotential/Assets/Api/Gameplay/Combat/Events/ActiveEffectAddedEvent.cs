using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Effects;

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
