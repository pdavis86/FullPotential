using FullPotential.Api.Logging;
using FullPotential.Api.Scenes;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Environment
{
    public class SceneService : ISceneService
    {
        private readonly IAuditor _logger;

        public SceneService(IAuditorFactory auditorFactory)
        {
            _logger = auditorFactory.Create(this);
        }

        public Vector3 GetPositionOnSolidObject(Vector3 startingPoint)
        {
            startingPoint.y += 10;

            if (Physics.Raycast(startingPoint, Vector3.down, out var hit, 30f))
            {
                return hit.point;
            }

            return startingPoint;
        }

        public Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, GameObject gameObject)
        {
            var collider = gameObject.GetComponent<Collider>();

            if (collider == null)
            {
                _logger.Warn("GameObject did not have any collider");
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
                _logger.Warn("Collider did not have any height");
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