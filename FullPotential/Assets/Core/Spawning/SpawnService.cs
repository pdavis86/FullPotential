using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using UnityEngine;

namespace FullPotential.Core.Spawning
{
    public class SpawnService
    {
        private readonly Collider _groundCollider;

        public SpawnService()
        {
            var ground = UnityHelper.GetObjectAtRoot("Environment").FindChildWithTag(Constants.Tags.Ground);
            _groundCollider = ground.GetComponent<Collider>();
        }

        public void AdjustPositionToBeAboveGround(Vector3 startingPoint, GameObject gameObject, bool removeHalfHeight = true)
        {
            var groundClosestPoint = _groundCollider.ClosestPointOnBounds(startingPoint);

            var adjustment = 0f;
            if (removeHalfHeight)
            {
                var gameObjectCollider = gameObject.GetComponent<Collider>();
                var gameObjectHeight = gameObjectCollider.bounds.max.y - gameObjectCollider.bounds.min.y;
                adjustment = gameObjectHeight / 2;
            }

            gameObject.transform.position = new Vector3(startingPoint.x, groundClosestPoint.y + adjustment, startingPoint.z);
        }
    }
}
