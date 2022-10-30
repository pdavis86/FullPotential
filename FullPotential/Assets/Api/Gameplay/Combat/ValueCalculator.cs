using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using System;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;
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

        private float GetTimeBetweenMaxAndMin(int attributeValue, float min, float max)
        {
            return (101 - attributeValue) / 100f * (max - min) + min;
        }

        public int GetDamageValueFromAttack(ItemBase itemUsed, int targetDefense)
        {
            //Even a small attack can still do damage
            var attackStrength = itemUsed?.Attributes.Strength ?? 1;
            var defenceRatio = 100f / (100 + targetDefense);
            var damageDealtBasic = Math.Ceiling(attackStrength * defenceRatio / 4f);

            if (itemUsed is Weapon weapon)
            {
                damageDealtBasic *= 2;

                if (weapon.IsTwoHanded)
                {
                    damageDealtBasic *= 2;
                }
            }

            return AddVariationToValue(damageDealtBasic);
        }

        public int GetDamageValueFromVelocity(Vector3 velocity)
        {
            var basicDamage = math.pow(velocity.magnitude, 1.9) * -1;
            return AddVariationToValue(basicDamage);
        }

        public float GetWeaponReloadTime(Attributes attributes)
        {
            var returnValue = GetTimeBetweenMaxAndMin(attributes.Recovery, 0.5f, 5);
            //var returnValue = (101 - attributes.Recovery) / 50f + 0.5f;
            //Debug.Log("GetReloadTime: " + returnValue);
            return returnValue;
        }

        public int GetWeaponAmmoMax(Attributes attributes)
        {
            var ammoCap = attributes.IsAutomatic ? 100 : 20;
            var returnValue = (int)(attributes.Efficiency / 100f * ammoCap);
            //Debug.Log("GetAmmoMax: " + returnValue);
            return returnValue;
        }

        public float GetSogContinuousRange(Attributes attributes)
        {
            var returnValue = attributes.Range / 100f * 10;
            //Debug.Log("GetContinuousRange: " + returnValue);
            return returnValue;
        }

        public float GetSogProjectileRange(Attributes attributes)
        {
            var returnValue = attributes.Range / 100f * 15 + 15;
            //Debug.Log("GetProjectileRange: " + returnValue);
            return returnValue;
        }

        public float GetSogProjectileSpeed(Attributes attributes)
        {
            var castSpeed = attributes.Speed / 50f;
            var returnValue = castSpeed < 0.5
                ? 0.5f
                : castSpeed;
            //Debug.Log("GetProjectileSpeed: " + returnValue);
            return returnValue;
        }

        public float GetSogChargeTime(Attributes attributes)
        {
            var returnValue = GetTimeBetweenMaxAndMin(attributes.Efficiency, 0, 2);
            //var returnValue = (101 - attributes.Efficiency) / 100f * 3;
            //Debug.Log("GetSogChargeTime: " + returnValue);
            return returnValue;
        }

        public float GetSogCooldownTime(Attributes attributes)
        {
            var returnValue = GetTimeBetweenMaxAndMin(attributes.Recovery, 0, 2);
            //var returnValue = (101 - attributes.Recovery) / 100f;
            //Debug.Log("GetSogChargeTime: " + returnValue);
            return returnValue;
        }

        public float GetEffectTimeBetween(Attributes attributes, float min = 0.5f, float max = 1.5f)
        {
            var returnValue = GetTimeBetweenMaxAndMin(attributes.Speed, min, max);
            //var returnValue = (101 - attributes.Speed) / 100f * (max - min) + min;
            //Debug.Log("GetTimeBetweenEffects: " + returnValue);
            return returnValue;
        }

        public float GetEffectDuration(Attributes attributes)
        {
            var returnValue = attributes.Duration / 10f;
            //Debug.Log("GetDuration: " + returnValue);
            return returnValue;
        }

        public float GetMovementForceValue(Attributes attributes, bool adjustForGravity)
        {
            var force = 4f * attributes.Strength;

            return adjustForGravity
                ? force * 1.2f
                : force;
        }

        public (int Change, DateTime Expiry) GetStatChangeAndExpiry(Attributes attributes, IStatEffect statEffect)
        {
            var change = AddVariationToValue(attributes.Strength / 5f);

            if (statEffect.Affect is Affect.PeriodicDecrease or Affect.SingleDecrease or Affect.TemporaryMaxDecrease)
            {
                change *= -1;
            }

            var timeToLive = statEffect.Affect == Affect.SingleIncrease || statEffect.Affect == Affect.SingleDecrease
                ? math.ceil(attributes.Duration / 50f)
                : math.ceil(attributes.Duration / 20f);

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }

        public (int Change, DateTime Expiry, float delay) GetStatChangeExpiryAndDelay(Attributes attributes, IStatEffect statEffect)
        {
            var (change, expiry) = GetStatChangeAndExpiry(attributes, statEffect);

            var delay = attributes.Recovery / 10;

            return (change, expiry, delay);
        }

        public (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(Attributes attributes, IAttributeEffect attributeEffect)
        {
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
