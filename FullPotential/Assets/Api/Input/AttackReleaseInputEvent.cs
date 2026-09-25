using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Input
{
    [RegisterEvent]
    public readonly struct AttackReleaseInputEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public AttackReleaseInputEvent(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
