using UnityEngine;

//todo: find a way to exclude "Assets" from namespaces
//todo: add namespace to every class (loads missing)
namespace FullPotential.Assets.Core.Extensions
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
