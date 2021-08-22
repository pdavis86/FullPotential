using UnityEngine;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Assets.Helpers
{
    public static class GameObjectHelper
    {
        public static bool IsDestroyed(GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }

    }
}
