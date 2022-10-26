using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using System;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Api.Gameplay.Combat
{
    public class ValueCalculator : IValueCalculator
    {
        public static readonly Random Random = new Random();

        public int AddVariationToValue(double basicValue)
        {
            var multiplier = (double)Random.Next(90, 111) / 100;
            var adder = Random.Next(0, 6);
            return (int)Math.Ceiling(basicValue / multiplier) + adder;
        }

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

        public float GetReloadTime(Attributes attributes)
        {
            var returnValue = (101 - attributes.Recovery) / 50f + 0.5f;
            //Debug.Log("GetReloadTime: " + returnValue);
            return returnValue;
        }

        public float GetProjectileRange(Attributes attributes)
        {
            var returnValue = attributes.Range / 100f * 15 + 15;
            //Debug.Log("GetProjectileRange: " + returnValue);
            return returnValue;
        }

        public float GetContinuousRange(Attributes attributes)
        {
            var returnValue = attributes.Range / 100f * 10;
            //Debug.Log("GetContinuousRange: " + returnValue);
            return returnValue;
        }

        public int GetAmmoMax(Attributes attributes)
        {
            var ammoCap = attributes.IsAutomatic ? 100 : 20;
            var returnValue = (int)(attributes.Efficiency / 100f * ammoCap);
            //Debug.Log("GetAmmoMax: " + returnValue);
            return returnValue;
        }

        public float GetTimeBetweenEffects(Attributes attributes, float min = 0.5f, float max = 1.5f)
        {
            //return 2 - (attributes.Duration / 100f);
            //return (101 - attributes.Recovery) / 200f + 0.5f;

            var returnValue = (101 - attributes.Speed) / 100f * (max - min) + min;
            //Debug.Log("GetTimeBetweenEffects: " + returnValue);
            return returnValue;
        }

        public float GetProjectileSpeed(Attributes attributes)
        {
            var castSpeed = attributes.Speed / 50f;
            var returnValue = castSpeed < 0.5
                ? 0.5f
                : castSpeed;
            //Debug.Log("GetProjectileSpeed: " + returnValue);
            return returnValue;
        }

        public float GetShapeLifetime(Attributes attributes)
        {
            var returnValue = attributes.Duration / 10f;
            //Debug.Log("GetShapeLifetime: " + returnValue);
            return returnValue;
        }

        public float GetDuration(Attributes attributes)
        {
            var returnValue = attributes.Duration / 10f;
            //Debug.Log("GetDuration: " + returnValue);
            return returnValue;
        }

        public float GetForceValue(Attributes attributes, bool adjustForGravity)
        {
            //todo: attribute-based force value
            return adjustForGravity
                ? 800f
                : 300f;
        }

        public (int Change, DateTime Expiry) GetStatChangeAndExpiry(Attributes attributes, IStatEffect statEffect)
        {
            //todo: attribute-based change and duration values

            var change = AddVariationToValue(attributes.Strength / 5f);

            if (statEffect.Affect is Affect.PeriodicDecrease or Affect.SingleDecrease or Affect.TemporaryMaxDecrease)
            {
                change *= -1;
            }

            var timeToLive = statEffect.Affect == Affect.SingleIncrease || statEffect.Affect == Affect.SingleDecrease
                ? 2f
                : 5f;

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }

        public (int Change, DateTime Expiry, float delay) GetStatChangeExpiryAndDelay(Attributes attributes, IStatEffect statEffect)
        {
            //todo: attribute-based delay value

            var (change, expiry) = GetStatChangeAndExpiry(attributes, statEffect);

            var delay = attributes.Recovery / 10;

            return (change, expiry, delay);
        }

        public (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(Attributes attributes, IAttributeEffect attributeEffect)
        {
            //todo: attribute-based change and duration values

            var change = AddVariationToValue(attributes.Strength / 5f);

            if (!attributeEffect.TemporaryMaxIncrease)
            {
                change *= -1;
            }

            var timeToLive = attributes.Duration / 2;

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }
    }
}
