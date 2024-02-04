using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class ResourceValueChangedEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterResourceValueChanged;

        private void HandleAfterResourceValueChanged(IEventHandlerArgs eventArgs)
        {
            var resourceChangeArgs = (ResourceValueChangedEventArgs)eventArgs;

            if (resourceChangeArgs.ResourceTypeId != BarrierChargeResource.TypeIdString)
            {
                return;
            }

            //todo: resource values should be stored on player or item then displayed on HUD

            var remainingCharge = resourceChangeArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);
            var showVisuals = remainingCharge > 0;

            resourceChangeArgs.LivingEntity.Inventory.ToggleEquippedItemVisuals(BarrierSlot.TypeIdString, showVisuals);
        }
    }
}
