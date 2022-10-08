using System;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Gameplay.Combat
{
    public static class AttributeCalculator
    {
        public static readonly Random Random = new Random();

        public static int GetAttackValue(Attributes? attributes, int targetDefense)
        {
            //Even a small attack can still do damage
            var attackStrength = attributes?.Strength ?? 1;
            var damageDealtBasic = attackStrength * 100f / (100 + targetDefense);

            //Throw in some variation
            var multiplier = (float)Random.Next(90, 111) / 100;
            var adder = Random.Next(0, 6);
            return (int)Math.Ceiling(damageDealtBasic / multiplier) + adder;
        }

        public static float GetForceValue(Attributes attributes, bool adjustForGravity)
        {
            //todo: attribute-based force value
            return adjustForGravity
                ? 1000f
                : 300f;
        }

        public static (int Change, DateTime Expiry) GetStatChangeAndExpiry(IStatEffect statEffect, Attributes attributes)
        {
            //todo: attribute-based change and duration values

            var change = 10;

            if (statEffect.Affect is Affect.PeriodicDecrease or Affect.SingleDecrease or Affect.TemporaryMaxDecrease)
            {
                change *= -1;
            }

            var timeToLive = statEffect.Affect == Affect.SingleIncrease || statEffect.Affect == Affect.SingleDecrease
                ? 2f
                : 5f;

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }

        public static (int Change, DateTime Expiry, float delay) GetStatChangeExpiryAndDelay(IStatEffect statEffect, Attributes attributes)
        {
            //todo: attribute-based delay value

            var (change, expiry) = GetStatChangeAndExpiry(statEffect, attributes);

            var delay = 0.5f;

            return (change, expiry, delay);
        }

        public static (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(IAttributeEffect attributeEffect, Attributes attributes)
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
