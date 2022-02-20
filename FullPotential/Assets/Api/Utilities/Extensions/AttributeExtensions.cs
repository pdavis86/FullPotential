using FullPotential.Api.Registry;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class AttributeExtensions
    {
        public static float GetReloadTime(this Attributes attributes)
        {
            return (101 - attributes.Recovery) / 50f + 0.5f;
        }

        public static float GetProjectileRange(this Attributes attributes)
        {
            return attributes.Range / 100f * 15 + 15;
        }

        public static float GetContinuousRange(this Attributes attributes)
        {
            return attributes.Range / 100f * 10;
        }

        public static int GetAmmoMax(this Attributes attributes)
        {
            var ammoCap = attributes.IsAutomatic ? 100 : 20;
            return (int)(attributes.Efficiency / 100f * ammoCap);
        }

        public static float GetTimeBetweenEffects(this Attributes attributes, float min = 0.5f, float max = 1.5f)
        {
            //return 2 - (attributes.Duration / 100f);
            //return (101 - attributes.Recovery) / 200f + 0.5f;
            return (101 - attributes.Speed) / 100f * (max - min) + min;
        }

        public static float GetProjectileSpeed(this Attributes attributes)
        {
            var castSpeed = attributes.Speed / 50f;
            return castSpeed < 0.5
                ? 0.5f
                : castSpeed;
        }

        public static float GetShapeLifetime(this Attributes attributes)
        {
            return attributes.Duration / 10f;
        }

    }
}
