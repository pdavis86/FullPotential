using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.EventArgs
{
    public class ResourceValueChangedEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public LivingEntityBase LivingEntity { get; }

        public string ResourceTypeId { get; }

        public int NewValue { get; }

        public int Change { get; set; }

        public ResourceValueChangedEventArgs(LivingEntityBase livingEntity, string resourceTypeId, int newValue, int change)
        {
            LivingEntity = livingEntity;
            ResourceTypeId = resourceTypeId;
            NewValue = newValue;
            Change = change;
        }
    }
}
