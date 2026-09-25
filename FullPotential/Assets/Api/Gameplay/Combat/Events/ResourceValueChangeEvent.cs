using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    [RegisterEvent]
    public readonly struct ResourceValueChangeEvent : IEvent
    {
        public LivingEntityBase LivingEntity { get; }

        public string ResourceTypeId { get; }

        public int NewValue { get; }

        public int Change { get; }

        public bool IsSelfInflicted { get; }

        public ResourceValueChangeEvent(LivingEntityBase livingEntity, string resourceTypeId, int newValue, int change, bool isSelfInflicted)
        {
            LivingEntity = livingEntity;
            ResourceTypeId = resourceTypeId;
            NewValue = newValue;
            Change = change;
            IsSelfInflicted = isSelfInflicted;
        }
    }
}
