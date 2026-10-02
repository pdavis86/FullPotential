using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public readonly struct ItemChargePercentageChangeEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public ItemChargePercentageChangeEvent(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
