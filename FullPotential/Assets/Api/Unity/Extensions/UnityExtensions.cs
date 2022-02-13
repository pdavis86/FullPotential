using UnityEngine;

namespace FullPotential.Api.Unity.Extensions
{
    public static class UnityExtensions
    {
        public static void DestroyChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }

    }
}
