using FullPotential.Api.Spawning;
using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using UnityEngine;

namespace FullPotential.Core.Spawning
{
    public class SpawnService : ISpawnService
    {
        private readonly Collider _groundCollider;

        public SpawnService()
        {
            var ground = GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.Environment).FindChildWithTag(Constants.Tags.Ground);
            _groundCollider = ground.GetComponent<Collider>();
        }

        private Vector3 GetPositionAboveGround(Vector3 startingPoint)
        {
            startingPoint.y += 10;
            return _groundCollider.ClosestPointOnBounds(startingPoint);
        }

        public void AdjustPositionToBeAboveGround(Vector3 startingPoint, GameObject gameObject, bool removeHalfHeight = true)
        {
            var groundClosestPoint = GetPositionAboveGround(startingPoint);

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
