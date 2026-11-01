using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public readonly struct ItemChargePercentageChangeEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public SlotId SlotId { get; }

        public ItemChargePercentageChangeEvent(FighterBase fighter, SlotId slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
