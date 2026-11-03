using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    public readonly struct SlotBusyChangeEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public SlotId SlotId { get; }

        public bool IsBusy { get; }

        public SlotBusyChangeEvent(FighterBase fighter, SlotId slotId, bool isBusy)
        {
            Fighter = fighter;
            SlotId = slotId;
            IsBusy = isBusy;
        }
    }
}
