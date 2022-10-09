using System;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class AttributeExtensions
    {
        public static float GetReloadTime(this Attributes attributes)
        {
            var returnValue = (101 - attributes.Recovery) / 50f + 0.5f;
            //Debug.Log("GetReloadTime: " + returnValue);
            return returnValue;
        }

        public static float GetProjectileRange(this Attributes attributes)
        {
            var returnValue = attributes.Range / 100f * 15 + 15;
            //Debug.Log("GetProjectileRange: " + returnValue);
            return returnValue;
        }

        public static float GetContinuousRange(this Attributes attributes)
        {
            var returnValue = attributes.Range / 100f * 10;
            //Debug.Log("GetContinuousRange: " + returnValue);
            return returnValue;
        }

        public static int GetAmmoMax(this Attributes attributes)
        {
            var ammoCap = attributes.IsAutomatic ? 100 : 20;
            var returnValue = (int)(attributes.Efficiency / 100f * ammoCap);
            //Debug.Log("GetAmmoMax: " + returnValue);
            return returnValue;
        }

        public static float GetTimeBetweenEffects(this Attributes attributes, float min = 0.5f, float max = 1.5f)
        {
            //return 2 - (attributes.Duration / 100f);
            //return (101 - attributes.Recovery) / 200f + 0.5f;

            var returnValue = (101 - attributes.Speed) / 100f * (max - min) + min;
            //Debug.Log("GetTimeBetweenEffects: " + returnValue);
            return returnValue;
        }

        public static float GetProjectileSpeed(this Attributes attributes)
        {
            var castSpeed = attributes.Speed / 50f;
            var returnValue = castSpeed < 0.5
                ? 0.5f
                : castSpeed;
            //Debug.Log("GetProjectileSpeed: " + returnValue);
            return returnValue;
        }

        public static float GetShapeLifetime(this Attributes attributes)
        {
            var returnValue = attributes.Duration / 10f;
            //Debug.Log("GetShapeLifetime: " + returnValue);
            return returnValue;
        }

        public static float GetDuration(this Attributes attributes)
        {
            var returnValue = attributes.Duration / 10f;
            //Debug.Log("GetDuration: " + returnValue);
            return returnValue;
        }

        public static float GetForceValue(this Attributes attributes, bool adjustForGravity)
        {
            //todo: attribute-based force value
            return adjustForGravity
                ? 1000f
                : 300f;
        }

        public static (int Change, DateTime Expiry) GetStatChangeAndExpiry(this Attributes attributes, IStatEffect statEffect)
        {
            //todo: attribute-based change and duration values

            var change = 5;

            if (statEffect.Affect is Affect.PeriodicDecrease or Affect.SingleDecrease or Affect.TemporaryMaxDecrease)
            {
                change *= -1;
            }

            var timeToLive = statEffect.Affect == Affect.SingleIncrease || statEffect.Affect == Affect.SingleDecrease
                ? 2f
                : 5f;

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }

        public static (int Change, DateTime Expiry, float delay) GetStatChangeExpiryAndDelay(this Attributes attributes, IStatEffect statEffect)
        {
            //todo: attribute-based delay value

            var (change, expiry) = attributes.GetStatChangeAndExpiry(statEffect);

            var delay = 0.5f;

            return (change, expiry, delay);
        }

        public static (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(this Attributes attributes, IAttributeEffect attributeEffect)
        {
            //todo: attribute-based change and duration values

            var change = 10;

            if (!attributeEffect.TemporaryMaxIncrease)
            {
                change *= -1;
            }

            var timeToLive = 5f;

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }
    }
}
