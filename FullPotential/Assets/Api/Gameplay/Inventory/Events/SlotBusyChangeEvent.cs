using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    [RegisterEvent]
    public readonly struct SlotBusyChangeEvent : IEvent
    {
        public string SlotId { get; }

        public bool IsBusy { get; }

        public SlotBusyChangeEvent(string slotId, bool isBusy)
        {
            SlotId = slotId;
            IsBusy = isBusy;
        }
    }
}
