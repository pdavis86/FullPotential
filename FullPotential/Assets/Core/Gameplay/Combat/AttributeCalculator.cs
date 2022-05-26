using System;
using FullPotential.Api.Registry;

namespace FullPotential.Core.Gameplay.Combat
{
    public static class AttributeCalculator
    {
        public static readonly Random Random = new Random();

        public static int GetAttackValue(Attributes? itemAttributes, int targetDefense)
        {
            //Even a small attack can still do damage
            var attackStrength = itemAttributes?.Strength ?? 1;
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
    }
}
