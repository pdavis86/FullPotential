using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Api.Input.Events
{
    public readonly struct AttackHoldInputEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public SlotId SlotId { get; }

        public AttackHoldInputEvent(FighterBase fighter, SlotId slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
