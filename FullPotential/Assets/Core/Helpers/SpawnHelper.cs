using System;
using UnityEngine;

namespace FullPotential.Core.Helpers
{
    public static class SpawnHelper
    {
        public static Vector3 GetAboveGroundPosition(Vector3 startingPoint, GameObject gameObject)
        {
            const float startingHeight = 100;
            const float maxDistance = 1000;

            //Debug.DrawRay(startingPoint, Vector3.down, Color.red, 15f);

            //A point far above the ground
            startingPoint.y = startingHeight;

            if (!Physics.Raycast(startingPoint, Vector3.down, out var hit, maxDistance, ~LayerMask.NameToLayer(Constants.Layers.Ground)))
            {
                throw new Exception("Could not find the ground");
            }

            //Debug.DrawRay(hit.point, Vector3.left, Color.cyan, 15f);
            //Debug.DrawRay(hit.point, Vector3.forward, Color.green, 15f);

            var groundYValue = gameObject.transform.position.y - gameObject.transform.localScale.y;
            //var collider = gameObject.GetComponent<Collider>();
            //var test1 = collider.bounds.size.y * gameObject.transform.localScale.y;

            //todo: chests are in the air and enemies are popping out of the ground :/

            return new Vector3(startingPoint.x, hit.point.y + (gameObject.transform.position.y / 2), startingPoint.z);
        }
    }
}
