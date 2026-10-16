using System;

using UnityEngine;

namespace FullPotential.Api.Gameplay.Player
{
    [Serializable]
    public struct BodyPartTransforms
    {
        public Transform Head;
        public Transform Body;
        public Transform LeftArm;
        public Transform RightArm;
    }
}
