using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public class ResourceValueChangedEventDefaultHandler : IEventHandler<ResourceValueChangedEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public Func<ResourceValueChangedEvent, UniTask> HandlerAsync => DefaultHandler;

        private UniTask DefaultHandler(ResourceValueChangedEvent eventArgs)
        {
            eventArgs.LivingEntity.UpdateResourceValue(eventArgs.ResourceTypeId, eventArgs.NewValue);
            return UniTask.CompletedTask;
        }
    }
}
