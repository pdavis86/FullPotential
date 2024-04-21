using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityDiedEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterResourceValueChanged;

        private void HandleAfterResourceValueChanged(IEventHandlerArgs eventArgs)
        {
            var changedArgs = (ResourceValueChangedEventArgs)eventArgs;

            if (changedArgs.NewValue != 0 || changedArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return;
            }

            changedArgs.LivingEntity.HandleDeath();
        }
    }
}
