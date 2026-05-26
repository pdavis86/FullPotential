using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class ChargeChangeEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => null;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => HandleAfterResourceValueChangedAsync;

        private UniTask HandleAfterResourceValueChangedAsync(IEventHandlerArgs eventArgs)
        {
            var resourceChangeArgs = (ResourceValueChangedEventArgs)eventArgs;

            if (resourceChangeArgs.ResourceTypeId != BarrierChargeResource.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            var remainingCharge = resourceChangeArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);
            var showVisuals = remainingCharge > 0;

            resourceChangeArgs.LivingEntity.Inventory.ToggleEquippedItemVisuals(BarrierSlot.TypeIdString, showVisuals);

            return UniTask.CompletedTask;
        }
    }
}
