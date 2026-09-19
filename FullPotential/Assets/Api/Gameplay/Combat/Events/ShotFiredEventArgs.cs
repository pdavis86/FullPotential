using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    // todo: move to standard
    [RegisterEvent]
    public struct ShotFiredEventArgs : IEventArgs
    {
        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public Vector3? StartPosition { get; }

        public Vector3? EndPosition { get; }

        public int? AmmoUsed { get; }

        public GameObject ObjectHit { get; }

        public ShotFiredEventArgs(
            FighterBase fighter,
            string slotId,
            Vector3? startPostion = null,
            Vector3? endPosition = null,
            int? ammoUsed = null,
            GameObject objectHit = null)
        {
            Fighter = fighter;
            SlotId = slotId;
            StartPosition = startPostion;
            EndPosition = endPosition;
            AmmoUsed = ammoUsed;
            ObjectHit = objectHit;
        }
    }
}
