using UnityEngine;

namespace FullPotential.Api.Spawning
{
    public interface ISpawnService
    {
        void AdjustPositionToBeAboveGround(Vector3 startingPoint, Transform transform, bool removeHalfHeight = true);

        void AdjustPositionToBeAboveGround(Vector3 startingPoint, Transform transform, float gameObjectHeight);
    }
}
