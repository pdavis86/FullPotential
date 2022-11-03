using FullPotential.Api.GameManagement;
using FullPotential.Api.Spawning;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Unity.Helpers;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Spawning
{
    public class SpawnService : ISpawnService
    {
        private readonly Collider _groundCollider;

        public SpawnService()
        {
            var ground = GameObjectHelper.GetObjectAtRoot(GameObjectNames.Environment).FindChildWithTag(Tags.Ground);
            _groundCollider = ground.GetComponent<Collider>();
        }

        private Vector3 GetPositionAboveGround(Vector3 startingPoint)
        {
            startingPoint.y += 10;
            return _groundCollider.ClosestPointOnBounds(startingPoint);
        }

        public void AdjustPositionToBeAboveGround(Vector3 startingPoint, Transform transform, bool removeHalfHeight = true)
        {
            var groundClosestPoint = GetPositionAboveGround(startingPoint);

            var adjustment = 0f;
            if (removeHalfHeight)
            {
                var gameObjectCollider = transform.GetComponent<Collider>();
                var gameObjectHeight = gameObjectCollider.bounds.max.y - gameObjectCollider.bounds.min.y;

                //Don't halve it as object can still end up in the floor
                adjustment = gameObjectHeight / 1.95f;
            }

            transform.position = new Vector3(startingPoint.x, groundClosestPoint.y + adjustment, startingPoint.z);
        }

        public void AdjustPositionToBeAboveGround(Vector3 startingPoint, Transform transform, float gameObjectHeight)
        {
            var groundClosestPoint = GetPositionAboveGround(startingPoint);

            //Don't halve it as object can still end up in the floor
            var adjustment = gameObjectHeight / 1.95f;

            transform.position = new Vector3(startingPoint.x, groundClosestPoint.y + adjustment, startingPoint.z);
        }
    }
}
