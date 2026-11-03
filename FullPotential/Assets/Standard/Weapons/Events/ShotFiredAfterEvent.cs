using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

using UnityEngine;

namespace FullPotential.Standard.Weapons.Events
{
    public readonly struct ShotFiredAfterEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public SlotId SlotId { get; }

        public Vector3? StartPosition { get; }

        public Vector3? EndPosition { get; }

        public int? AmmoUsed { get; }

        public GameObject ObjectHit { get; }

        public ShotFiredAfterEvent(
            FighterBase fighter,
            SlotId slotId,
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
