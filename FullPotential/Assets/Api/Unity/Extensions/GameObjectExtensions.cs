using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Unity.Extensions
{
    public static class GameObjectExtensions
    {
        public static bool CompareTagAny(this GameObject gameObject, params string[] tags)
        {
            foreach (var tag in tags)
            {
                if (gameObject.CompareTag(tag))
                {
                    return true;
                }
            }
            return false;
        }

        public static GameObject FindChildWithTag(this GameObject gameObject, string tag)
        {
            foreach (Transform tr in gameObject.transform)
            {
                if (tr.CompareTag(tag))
                {
                    return tr.gameObject;
                }
            }
            return null;
        }

        public static Transform FindInDescendants(this GameObject gameObject, string name)
        {
            var child = gameObject.transform.Find(name);

            if (child != null)
            {
                return child;
            }

            foreach (Transform tr in gameObject.transform)
            {
                var subChild = tr.Find(name);
                if (subChild != null)
                {
                    return subChild;
                }
            }
            return null;
        }

        public static void DeleteAllChildren(this GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                Object.Destroy(child.gameObject);
            }
        }

        public static void NetworkSpawn(this GameObject gameObject, bool warnOnFailure = true)
        {
            var networkObject = gameObject.GetComponent<NetworkObject>();

            if (warnOnFailure && networkObject == null)
            {
                Debug.LogWarning($"Cannot network spawn {gameObject.name} as it does not have a NetworkObject component");
                return;
            }

            networkObject?.Spawn(true);
        }

        public static void SetGameLayerRecursive(this GameObject gameObject, int layer)
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

        public static GameObject GetClosestParentWithTag(this GameObject gameObject, string tag)
        {
            var current = gameObject.transform;
            do
            {
                current = current.parent;

            } while (current != null && !current.CompareTag(tag));

            return current == null ? null : current.gameObject;
        }

    }
}
