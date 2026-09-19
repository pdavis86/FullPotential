using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    [RegisterEvent]
    public struct ResourceValueChangedEventArgs : IEventArgs
    {
        public LivingEntityBase LivingEntity { get; }

        public string ResourceTypeId { get; }

        public int NewValue { get; }

        public int Change { get; set; }

        public bool IsSelfInflicted { get; }

        public ResourceValueChangedEventArgs(LivingEntityBase livingEntity, string resourceTypeId, int newValue, int change, bool isSelfInflicted)
        {
            LivingEntity = livingEntity;
            ResourceTypeId = resourceTypeId;
            NewValue = newValue;
            Change = change;
            IsSelfInflicted = isSelfInflicted;
        }
    }
}
