using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Registry.Resources;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class HealthChangeEventHandler : IEventHandler
    {
        public const string CustomDataKeyLastHit = "LastHit";

        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => HandleBeforeHealthChange;

        public Action<IEventHandlerArgs> AfterHandler => null;

        private void HandleBeforeHealthChange(IEventHandlerArgs eventArgs)
        {
            var resourceChangeArgs = (ResourceValueChangedEventArgs)eventArgs;

            if (resourceChangeArgs.ResourceTypeId != ResourceTypeIds.HealthId
                || resourceChangeArgs.Change >= 0)
            {
                return;
            }

            var barrier = (Api.Items.Types.SpecialGear)resourceChangeArgs.LivingEntity.Inventory.GetItemInSlot(BarrierSlot.TypeIdString);

            if (barrier == null)
            {
                return;
            }

            var barrierCharge = resourceChangeArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);

            if (barrierCharge <= 0)
            {
                //Debug.Log("Barrier depleted. Taking full damage");
                return;
            }

            barrier.SetCustomData(CustomDataKeyLastHit, DateTime.UtcNow.ToString("u"));

            resourceChangeArgs.LivingEntity.AdjustResourceValue(BarrierChargeResource.TypeIdString, resourceChangeArgs.Change);

            if (barrierCharge < Math.Abs(resourceChangeArgs.Change))
            {
                //Debug.Log("Barrier nearly depleted. Taking partial damage");
                resourceChangeArgs.Change += barrierCharge;
                return;
            }

            //Debug.Log("Barrier OK. Taking no damage");
            eventArgs.IsDefaultHandlerCancelled = true;
        }
    }
}
