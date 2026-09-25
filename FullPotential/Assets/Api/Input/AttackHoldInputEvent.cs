using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Input
{
    [RegisterEvent]
    public readonly struct AttackHoldInputEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public AttackHoldInputEvent(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
