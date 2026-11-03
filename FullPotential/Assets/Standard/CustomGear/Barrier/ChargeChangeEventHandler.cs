using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Standard.SpecialSlots;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.CustomGear.Barrier
{
    public class ChargeChangeEventHandler : IEventHandler<ValuePoolValueChangeEvent>
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.Late;

        public UniTask<HandlerResult> HandleEventAsync(ValuePoolValueChangeEvent eventArgs)
        {
            if (eventArgs.ValuePoolTypeId != BarrierChargeValuePool.TypeIdString)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var remainingCharge = eventArgs.LivingEntity.GetValuePoolValue(BarrierChargeValuePool.TypeIdString);
            var showVisuals = remainingCharge > 0;

            eventArgs.LivingEntity.Inventory.ToggleEquippedItemVisuals(SlotId.FromString(BarrierSlot.TypeIdString), showVisuals);

            return UniTask.FromResult(new HandlerResult());
        }
    }
}
