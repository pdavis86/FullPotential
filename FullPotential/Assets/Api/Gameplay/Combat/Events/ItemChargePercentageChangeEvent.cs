using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    [RegisterEvent]
    public readonly struct ItemChargePercentageChangeEvent : IEvent
    {
        public string SlotId { get; }

        public ItemChargePercentageChangeEvent(string slotId)
        {
            SlotId = slotId;
        }
    }
}
