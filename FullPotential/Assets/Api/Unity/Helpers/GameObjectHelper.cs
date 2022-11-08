using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Unity.Helpers
{
    public static class GameObjectHelper
    {
        public static GameObject GetObjectAtRoot(string name)
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == name);
        }

        public static void SetGameLayerRecursive(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = layer;

                var hasChildren = child.GetComponentInChildren<Transform>();
                if (hasChildren != null)
                {
                    SetGameLayerRecursive(child.gameObject, layer);
                }
            }
        }

        public static GameObject ClosestParentWithTag(GameObject gameObject, string tag)
        {
            var current = gameObject.transform;
            do
            {
                current = current.parent;

            } while (current != null && !current.CompareTag(tag));

            return current == null ? null : current.gameObject;
        }

        public static Vector3[] GetBoxColliderVertices(BoxCollider collider)
        {
            var bounds = collider.bounds;

            return new[]
            {
                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
            };
        }

    }
}
