using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Api.Input.Events
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
