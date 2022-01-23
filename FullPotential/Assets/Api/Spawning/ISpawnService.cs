using UnityEngine;

namespace FullPotential.Api.Spawning
{
    public interface ISpawnService
    {
        void AdjustPositionToBeAboveGround(Vector3 startingPoint, GameObject gameObject, bool removeHalfHeight = true);
    }
}
