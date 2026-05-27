using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    [RegisterEvent(LivingEntityBase.ResourceValueChangeEventId)]
    public class LivingEntityHealthChangedEventHandler : IEventHandler<ResourceValueChangedEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Func<ResourceValueChangedEventArgs, UniTask> BeforeHandlerAsync => null;

        public Func<ResourceValueChangedEventArgs, UniTask> AfterHandlerAsync => HandleAfterValueChangedAsync;

        private UniTask HandleAfterValueChangedAsync(ResourceValueChangedEventArgs eventArgs)
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
