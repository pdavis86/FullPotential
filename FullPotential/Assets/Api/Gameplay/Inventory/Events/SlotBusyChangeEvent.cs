using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    [RegisterEvent]
    public readonly struct SlotBusyChangeEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public bool IsBusy { get; }

        public SlotBusyChangeEvent(FighterBase fighter, string slotId, bool isBusy)
        {
            Fighter = fighter;
            SlotId = slotId;
            IsBusy = isBusy;
        }
    }
}
