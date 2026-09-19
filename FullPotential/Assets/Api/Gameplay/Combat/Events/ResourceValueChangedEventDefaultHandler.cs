using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public class ResourceValueChangedEventDefaultHandler : IEventHandler<ResourceValueChangedEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public Func<ResourceValueChangedEventArgs, UniTask<HandlerResult>> HandlerAsync => DefaultHandlerAsync;

        private UniTask<HandlerResult> DefaultHandlerAsync(ResourceValueChangedEventArgs eventArgs)
        {
            eventArgs.LivingEntity.UpdateResourceValue(eventArgs.ResourceTypeId, eventArgs.NewValue);
            return UniTask.FromResult(new HandlerResult());
        }
    }
}
