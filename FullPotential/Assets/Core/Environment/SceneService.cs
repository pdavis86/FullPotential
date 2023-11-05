using FullPotential.Api.Scenes;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Environment
{
    public class SceneService : ISceneService
    {
        public Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, GameObject gameObject)
        {
            var collider = gameObject.GetComponent<Collider>();

            if (collider == null)
            {
                Debug.LogWarning("GamObject did not have any collider");
                return startingPoint;
            }

            return GetHeightAdjustedPosition(startingPoint, collider);
        }

        public Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, Collider collider)
        {
            var gameObjectHeight = collider.bounds.max.y - collider.bounds.min.y;

            return GetHeightAdjustedPosition(startingPoint, gameObjectHeight);
        }

        public Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, float gameObjectHeight)
        {
            if (gameObjectHeight == 0)
            {
                Debug.LogWarning("Collider did not have any height");
                return startingPoint;
            }

            var adjustment = gameObjectHeight / 2;

            return new Vector3(
                startingPoint.x,
                startingPoint.y + adjustment,
                startingPoint.z);
        }
    }
}