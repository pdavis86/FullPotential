using FullPotential.Api.GameManagement;
using FullPotential.Api.Scenes;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Unity.Services;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Environment
{
    public class SceneService : ISceneService
    {
        private readonly Collider _groundCollider;

        public SceneService(IUnityHelperUtilities unityHelperUtilities)
        {
            var ground = unityHelperUtilities.GetObjectAtRoot(GameObjectNames.Environment).FindChildWithTag(Tags.Ground);
            _groundCollider = ground.GetComponent<Collider>();
        }

        public Vector3 GetPositionAboveGround(Vector3 startingPoint)
        {
            startingPoint.y += 10;
            return _groundCollider.ClosestPointOnBounds(startingPoint);
        }

        public Vector3 GetPositionAboveGround(Vector3 startingPoint, GameObject gameObject)
        {
            var collider = gameObject.GetComponent<Collider>();

            if (collider == null)
            {
                Debug.LogWarning("GamObject did not have any collider");
                return startingPoint;
            }

            return GetPositionAboveGround(startingPoint, collider);
        }

        public Vector3 GetPositionAboveGround(Vector3 startingPoint, Collider collider)
        {
            var gameObjectHeight = collider.bounds.max.y - collider.bounds.min.y;

            return GetPositionAboveGround(startingPoint, gameObjectHeight);
        }

        public Vector3 GetPositionAboveGround(Vector3 startingPoint, float gameObjectHeight)
        {
            var groundClosestPoint = GetPositionAboveGround(startingPoint);

            if (gameObjectHeight == 0)
            {
                Debug.LogWarning("Collider did not have any height");
                return startingPoint;
            }

            var adjustment = gameObjectHeight / 2;

            return new Vector3(
                startingPoint.x,
                groundClosestPoint.y + adjustment,
                startingPoint.z);
        }
    }
}