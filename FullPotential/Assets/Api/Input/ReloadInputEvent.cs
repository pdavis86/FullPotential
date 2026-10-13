using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Input
{
    public readonly struct ReloadInputEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public ReloadInputEvent(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
