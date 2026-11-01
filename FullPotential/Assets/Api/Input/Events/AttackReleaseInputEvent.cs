using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Api.Input.Events
{
    public readonly struct AttackReleaseInputEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public SlotId SlotId { get; }

        public AttackReleaseInputEvent(FighterBase fighter, SlotId slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
