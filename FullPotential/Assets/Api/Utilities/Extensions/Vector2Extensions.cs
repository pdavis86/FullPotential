using UnityEngine;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class Vector2Extensions
    {
        public static float ClockwiseAngleTo(this Vector2 a, Vector2 b)
        {
            var angle = Vector2.SignedAngle(b.normalized, a);

            if (angle < 0)
            {
                return 360 - angle * -1;
            }

            return angle;
        }
    }
}
