using System;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Crafting;
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
    }
}
