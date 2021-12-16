using UnityEngine;

namespace FullPotential.Core.Extensions
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

    }
}
