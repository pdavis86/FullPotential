using UnityEngine;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 AddNoiseOnAngle(this Vector3 original, float minDegrees, float maxDegrees)
        {
            var xNoise = Random.Range(minDegrees, maxDegrees);
            var yNoise = Random.Range(minDegrees, maxDegrees);
            var zNoise = Random.Range(minDegrees, maxDegrees);

            // Convert Angle to Vector3
            return original + new Vector3(
                Mathf.Sin(2 * Mathf.PI * xNoise / 360),
                Mathf.Sin(2 * Mathf.PI * yNoise / 360),
                Mathf.Sin(2 * Mathf.PI * zNoise / 360)
            );
        }
    }
}
