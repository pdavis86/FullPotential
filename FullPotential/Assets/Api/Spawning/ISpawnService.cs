using System;
using UnityEngine;

namespace FullPotential.Api.Spawning
{
    public interface ISpawnService
    {
        Vector3 GetPositionAboveGround(Vector3 startingPoint, Collider collider);

        [Obsolete("Instead of doing the adjustment, return the position")]
        void AdjustPositionToBeAboveGround(Vector3 startingPoint, Transform transform, bool removeHalfHeight = true);

        [Obsolete("Instead of doing the adjustment, return the position")]
        void AdjustPositionToBeAboveGround(Vector3 startingPoint, Transform transform, float gameObjectHeight);
    }
}
