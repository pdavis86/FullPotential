using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Standard.TargetingVisuals
{
    public interface ITargetingVisualsBehaviour
    {
        Vector3 StartPosition { get; set; }

        Vector3 StartDirection { get; set; }

        bool IsLocalOwner { get; set; }

        void UpdateVisuals(bool isHitting, Vector3 hitPoint, Vector3 origin, Vector3 direction, float maxRange);
    }
}
