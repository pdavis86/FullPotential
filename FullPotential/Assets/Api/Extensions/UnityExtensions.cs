using UnityEngine;

namespace FullPotential.Api.Extensions
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
