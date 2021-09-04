using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Assets.Core.Helpers
{
    public static class GameObjectHelper
    {
        public static void SetGameLayerRecursive(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = layer;

                Transform hasChildren = child.GetComponentInChildren<Transform>();
                if (hasChildren != null)
                {
                    SetGameLayerRecursive(child.gameObject, layer);
                }
            }
        }

        public static bool IsDestroyed(GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }

    }
}
