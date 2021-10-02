using UnityEngine;

namespace FullPotential.Assets.Extensions
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

    }
}
