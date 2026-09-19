using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    [RegisterEvent("34372a74-abf3-44eb-8598-4427a82f29ab")]
    public class ResourceValueChangedEvent : IEvent
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public LivingEntityBase LivingEntity { get; }

        public string ResourceTypeId { get; }

        public int NewValue { get; }

        public int Change { get; set; }

        public bool IsSelfInflicted { get; set; }

        public ResourceValueChangedEvent(LivingEntityBase livingEntity, string resourceTypeId, int newValue, int change, bool isSelfInflicted)
        {
            LivingEntity = livingEntity;
            ResourceTypeId = resourceTypeId;
            NewValue = newValue;
            Change = change;
            IsSelfInflicted = isSelfInflicted;
        }
    }
}
