using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    [RegisterEvent(LivingEntityBase.ResourceValueChangeEventId)]
    public class LivingEntityDiedEventHandler : IEventHandler<ResourceValueChangedEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<ResourceValueChangedEventArgs, UniTask> BeforeHandlerAsync => null;

        public Func<ResourceValueChangedEventArgs, UniTask> AfterHandlerAsync => HandleAfterResourceValueChangedAsync;

        private UniTask HandleAfterResourceValueChangedAsync(ResourceValueChangedEventArgs eventArgs)
        {
            if (eventArgs.NewValue > 0 || eventArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return UniTask.CompletedTask;
            }

            eventArgs.LivingEntity.HandleDeath();

            return UniTask.CompletedTask;
        }
    }
}
