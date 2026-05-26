using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityDiedEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => null;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => HandleAfterResourceValueChangedAsync;

        private UniTask HandleAfterResourceValueChangedAsync(IEventHandlerArgs eventArgs)
        {
            var changedArgs = (ResourceValueChangedEventArgs)eventArgs;

            if (changedArgs.NewValue > 0 || changedArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return UniTask.CompletedTask;
            }

            changedArgs.LivingEntity.HandleDeath();

            return UniTask.CompletedTask;
        }
    }
}
