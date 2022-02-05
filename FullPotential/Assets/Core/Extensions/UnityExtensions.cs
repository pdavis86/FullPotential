using UnityEngine;

namespace FullPotential.Core.Extensions
{
    public static class UnityExtensions
    {
        public static void Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                GameObject.Destroy(child.gameObject);
            }
        }

    }
}
