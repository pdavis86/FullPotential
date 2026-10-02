using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public readonly struct ResourceValueChangeEvent : IEvent
    {
        public LivingEntityBase LivingEntity { get; }

        public string ResourceTypeId { get; }

        public int NewValue { get; }

        public int Change { get; }

        public int MaxValue { get; }

        public string SourceEntityName { get; }

        public string SourceItemName { get; }

        public ResourceValueChangeEvent(
            LivingEntityBase livingEntity,
            string resourceTypeId,
            int newValue,
            int change,
            int maxValue,
            string sourceEntityName,
            string sourceItemName)
        {
            LivingEntity = livingEntity;
            ResourceTypeId = resourceTypeId;
            NewValue = newValue;
            Change = change;
            MaxValue = maxValue;
            SourceEntityName = sourceEntityName;
            SourceItemName = sourceItemName;
        }
    }
}
