using UnityEngine;

namespace FullPotential.Api.Extensions
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

    }
}
