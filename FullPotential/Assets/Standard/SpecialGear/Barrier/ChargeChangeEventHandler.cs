using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class ChargeChangeEventHandler : IEventHandler<ResourceValueChangedEvent>
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Func<ResourceValueChangedEvent, UniTask> BeforeHandlerAsync => null;

        public Func<ResourceValueChangedEvent, UniTask> AfterHandlerAsync => HandleAfterResourceValueChangedAsync;

        private UniTask HandleAfterResourceValueChangedAsync(ResourceValueChangedEvent eventArgs)
        {
            if (eventArgs.ResourceTypeId != BarrierChargeResource.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            var remainingCharge = eventArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);
            var showVisuals = remainingCharge > 0;

            eventArgs.LivingEntity.Inventory.ToggleEquippedItemVisuals(BarrierSlot.TypeIdString, showVisuals);

            return UniTask.CompletedTask;
        }
    }
}
