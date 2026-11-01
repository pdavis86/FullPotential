using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public readonly struct ValuePoolValueChangeEvent : IEvent
    {
        public LivingEntityBase LivingEntity { get; }

        public string ValuePoolTypeId { get; }

        public int NewValue { get; }

        public int Change { get; }

        public int MaxValue { get; }

        public string SourceEntityName { get; }

        public string SourceItemName { get; }

        public ValuePoolValueChangeEvent(
            LivingEntityBase livingEntity,
            string valuePoolTypeId,
            int newValue,
            int change,
            int maxValue,
            string sourceEntityName,
            string sourceItemName)
        {
            LivingEntity = livingEntity;
            ValuePoolTypeId = valuePoolTypeId;
            NewValue = newValue;
            Change = change;
            MaxValue = maxValue;
            SourceEntityName = sourceEntityName;
            SourceItemName = sourceItemName;
        }
    }
}
