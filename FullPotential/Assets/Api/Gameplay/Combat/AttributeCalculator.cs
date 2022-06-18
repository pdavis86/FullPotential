using System;
using FullPotential.Api.Registry;

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

        public static float GetForceValue(Attributes attributes)
        {
            //todo: attribute-based force value
            return 100f;
        }

        public static (int Change, DateTime Expiry) GetStatChangeAndExpiry(Attributes attributes)
        {
            //const float displayTimeForSingleChangeToStat = 2f;
            //todo: attribute-based change and duration values
            return (10, DateTime.Now.AddSeconds(5f));
        }
    }
}
