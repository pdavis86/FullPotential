using UnityEngine;

namespace FullPotential.Core.Extensions
{
    public static class UnityExtensions
    {
        public static void Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }

    }
}
