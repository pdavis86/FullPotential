using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    [RegisterEvent("2337f94e-5a7d-4e02-b1c8-1b5e9934a3ce")]
    public class ReloadEvent : IEvent
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public ReloadEvent(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
