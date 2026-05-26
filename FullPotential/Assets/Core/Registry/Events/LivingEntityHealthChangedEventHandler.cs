using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    internal class LivingEntityHealthChangedEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => null;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => HandleAfterValueChangedAsync;

        private UniTask HandleAfterValueChangedAsync(IEventHandlerArgs eventArgs)
        {
            var valueChangedArgs = (ResourceValueChangedEventArgs)eventArgs;

            if (valueChangedArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return UniTask.CompletedTask;
            }

            valueChangedArgs.LivingEntity.UpdateUiHealthAndDefenceValues();

            return UniTask.CompletedTask;
        }
    }
}
