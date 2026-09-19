using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityDiedEventHandler : IEventHandler<ResourceValueChangedEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.After;

        public Func<ResourceValueChangedEventArgs, UniTask<HandlerResult>> HandlerAsync => HandleAfterResourceValueChangedAsync;

        private UniTask<HandlerResult> HandleAfterResourceValueChangedAsync(ResourceValueChangedEventArgs eventArgs)
        {
            if (eventArgs.NewValue > 0 || eventArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            eventArgs.LivingEntity.HandleDeath();

            return UniTask.FromResult(new HandlerResult());
        }
    }
}
