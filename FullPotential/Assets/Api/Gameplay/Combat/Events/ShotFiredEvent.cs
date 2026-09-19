using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    [RegisterEvent("f01cd95a-67cc-4f38-a394-5a69eaa721c6")]
    public class ShotFiredEvent : IEvent
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public Vector3 StartPosition { get; set; }

        public Vector3 EndPosition { get; set; }

        public int AmmoUsed { get; set; }

        public GameObject ObjectHit { get; set; }

        public ShotFiredEvent(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
