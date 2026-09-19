using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityHealthChangedEventHandler : IEventHandler<ResourceValueChangedEvent>
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Func<ResourceValueChangedEvent, UniTask> BeforeHandlerAsync => null;

        public Func<ResourceValueChangedEvent, UniTask> AfterHandlerAsync => HandleAfterValueChangedAsync;

        private UniTask HandleAfterValueChangedAsync(ResourceValueChangedEvent eventArgs)
        {
            if (eventArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return UniTask.CompletedTask;
            }

            eventArgs.LivingEntity.UpdateUiHealthAndDefenceValues();

            return UniTask.CompletedTask;
        }
    }
}
