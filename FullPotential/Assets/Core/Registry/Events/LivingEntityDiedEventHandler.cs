using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityDiedEventHandler : IEventHandler<ResourceValueChangedEvent>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.After;

        public Func<ResourceValueChangedEvent, UniTask> HandlerAsync => HandleAfterResourceValueChangedAsync;

        private UniTask HandleAfterResourceValueChangedAsync(ResourceValueChangedEvent eventArgs)
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
