using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Input
{
    [RegisterEvent]
    public struct AttackHoldEventArgs : IEventArgs
    {
        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public AttackHoldEventArgs(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
