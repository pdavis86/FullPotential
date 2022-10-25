using System;
using FullPotential.Api.Registry;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

namespace FullPotential.Api.Gameplay.Combat
{
    public class ValueCalculator : IValueCalculator
    {
        public static readonly Random Random = new Random();

        public int GetAttackValue(Attributes? attributes, int targetDefense)
        {
            //Even a small attack can still do damage
            var attackStrength = attributes?.Strength ?? 1;
            var damageDealtBasic = attackStrength * 100f / (100 + targetDefense);

            return AddVariationToValue(damageDealtBasic);
        }

        public int GetVelocityDamage(Vector3 velocity)
        {
            var basicDamage = math.pow(velocity.magnitude, 1.9) * -1;
            return AddVariationToValue(basicDamage);
        }

        public int AddVariationToValue(double basicValue)
        {
            var multiplier = (double)Random.Next(90, 111) / 100;
            var adder = Random.Next(0, 6);
            return (int)Math.Ceiling(basicValue / multiplier) + adder;
        }
    }
}
