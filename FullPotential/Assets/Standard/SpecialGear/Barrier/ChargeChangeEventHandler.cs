using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class ChargeChangeEventHandler : IEventHandler<ResourceValueChangeEvent>
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.Late;

        public UniTask<HandlerResult> HandleEventAsync(ResourceValueChangeEvent eventArgs)
        {
            if (eventArgs.ResourceTypeId != BarrierChargeResource.TypeIdString)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var remainingCharge = eventArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);
            var showVisuals = remainingCharge > 0;

            eventArgs.LivingEntity.Inventory.ToggleEquippedItemVisuals(BarrierSlot.TypeIdString, showVisuals);

            return UniTask.FromResult(new HandlerResult());
        }
    }
}
