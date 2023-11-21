using FullPotential.Api.Gameplay.Events;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat.EventArgs
{
    public class ShotFiredEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public IFighter Fighter { get; }

        public bool IsLeftHand { get; }

        public Vector3 StartPosition { get; set; }

        public Vector3 EndPosition { get; set; }

        public ShotFiredEventArgs(IFighter fighter, bool isLeftHand)
        {
            Fighter = fighter;
            IsLeftHand = isLeftHand;
        }
    }
}
