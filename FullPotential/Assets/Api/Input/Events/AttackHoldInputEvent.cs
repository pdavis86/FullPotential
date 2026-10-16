using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Api.Input.Events
{
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
