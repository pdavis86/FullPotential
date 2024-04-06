using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat.EventArgs
{
    public class ShotFiredEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public FighterBase Fighter { get; }

        public bool IsLeftHand { get; }

        public Vector3 StartPosition { get; set; }

        public Vector3 EndPosition { get; set; }

        public int AmmoUsed { get; set; }

        public GameObject ObjectHit { get; set; }

        public ShotFiredEventArgs(FighterBase fighter, bool isLeftHand)
        {
            Fighter = fighter;
            IsLeftHand = isLeftHand;
        }
    }
}
