using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityDiedEventHandler : IEventHandler
    {
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
