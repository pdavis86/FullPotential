using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class ClientHealthChangeEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterHealthChange;

        private void HandleAfterHealthChange(IEventHandlerArgs eventArgs)
        {
            var healthChangeArgs = (HealthChangeEventArgs)eventArgs;

            //todo: resource values should be stored on player or item then displayed on HUD

            var showVisuals = healthChangeArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString) > 0;

            healthChangeArgs.LivingEntity.Inventory.ToggleEquippedItemVisuals(BarrierSlot.TypeIdString, showVisuals);
        }
    }
}
