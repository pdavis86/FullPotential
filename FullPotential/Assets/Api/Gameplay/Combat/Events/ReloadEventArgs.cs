using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    public class ReloadEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public ReloadEventArgs(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
