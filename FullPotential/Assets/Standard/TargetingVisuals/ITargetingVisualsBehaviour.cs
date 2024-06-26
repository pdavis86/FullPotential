﻿using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Standard.TargetingVisuals
{
    public interface ITargetingVisualsBehaviour
    {
        public Vector3 StartPosition { get; set; }

        public Vector3 StartDirection { get; set; }

        public bool IsLocalOwner { get; set; }

        public void UpdateVisuals(bool isHitting, Vector3 hitPoint, Vector3 origin, Vector3 direction, float maxRange);
    }
}
