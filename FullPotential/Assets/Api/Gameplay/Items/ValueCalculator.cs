using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Weapons;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Registry.Effects;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Api.Gameplay.Items
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

        public int GetDamageValueFromAttack(ItemBase itemUsed, int targetDefense, bool addVariation = true)
        {
            var weapon = itemUsed as Weapon;
            var weaponCategory = (weapon?.RegistryType as IGearWeapon)?.Category;

            float attackStrength = itemUsed?.Attributes.Strength ?? 1;
            var defenceRatio = 100f / (100 + targetDefense);
            
            //todo: review
            if (weaponCategory == IGearWeapon.WeaponCategory.Ranged)
            {
                attackStrength *= weapon.GetFireRate();
            }

            //Even a small attack can still do damage
            var damageDealtBasic = Math.Ceiling(attackStrength * defenceRatio / 4f);

            if (weapon != null && weapon.IsTwoHanded)
            {
                damageDealtBasic *= 2;
            }

            if (weaponCategory == IGearWeapon.WeaponCategory.Melee)
            {
                damageDealtBasic *= 2;
            }

            if (!addVariation)
            {
                return (int)damageDealtBasic;
            }

            return AddVariationToValue(damageDealtBasic);
        }

        public int GetDamageValueFromVelocity(Vector3 velocity)
        {
            var basicDamage = math.pow(velocity.magnitude, 1.9) * -1;
            return AddVariationToValue(basicDamage);
        }

        public float GetEffectTimeBetween(ItemBase itemUsed, float min = 0.5f, float max = 1.5f)
        {
            var returnValue = ItemBase.GetValueInRangeHighLow(itemUsed.Attributes.Speed, min, max);
            //Debug.Log("GetTimeBetweenEffects: " + returnValue);
            return returnValue;
        }

        public float GetEffectDuration(ItemBase itemUsed)
        {
            var returnValue = itemUsed.Attributes.Duration / 10f;
            //Debug.Log("GetDuration: " + returnValue);
            return returnValue;
        }

        public float GetMovementForceValue(ItemBase itemUsed, bool adjustForGravity)
        {
            var force = 4f * itemUsed.Attributes.Strength;

            return adjustForGravity
                ? force * 1.2f
                : force;
        }

        public (int Change, DateTime Expiry) GetStatChangeAndExpiry(ItemBase itemUsed, IStatEffect statEffect)
        {
            var change = AddVariationToValue(itemUsed.Attributes.Strength / 5f);

            if (statEffect.Affect is Affect.PeriodicDecrease or Affect.SingleDecrease or Affect.TemporaryMaxDecrease)
            {
                change *= -1;
            }

            var timeToLive = statEffect.Affect == Affect.SingleIncrease || statEffect.Affect == Affect.SingleDecrease
                ? math.ceil(itemUsed.Attributes.Duration / 50f)
                : math.ceil(itemUsed.Attributes.Duration / 20f);

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }

        public (int Change, DateTime Expiry, float delay) GetStatChangeExpiryAndDelay(ItemBase itemUsed, IStatEffect statEffect)
        {
            var (change, expiry) = GetStatChangeAndExpiry(itemUsed, statEffect);

            var delay = itemUsed.Attributes.Recovery / 10;

            return (change, expiry, delay);
        }

        public (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(ItemBase itemUsed, IAttributeEffect attributeEffect)
        {
            var change = AddVariationToValue(itemUsed.Attributes.Strength / 5f);

            if (!attributeEffect.TemporaryMaxIncrease)
            {
                change *= -1;
            }

            var timeToLive = itemUsed.Attributes.Duration / 2;

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }
    }
}
