using System;
using System.Linq;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gear;
using UnityEngine;

namespace FullPotential.Core.Registry.Combat
{
    public class HurtEffectComputation : IEffectComputation
    {
        private readonly ITypeRegistry _typeRegistry;

        private static readonly Guid Id = new Guid("c06fd4bf-a006-4fa2-966e-8f368c680818");

        public Guid TypeId => Id;

        public string EffectTypeId => EffectTypeIds.HurtId;

        public HurtEffectComputation(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }
        
        public CombatResult GetCombatResult(FighterBase sourceFighter, CombatItemBase itemUsed, LivingEntityBase targetLivingEntity)
        {
            float attackStrength = itemUsed?.Attributes.Strength ?? sourceFighter.GetAttributeValue(AttributeAffected.Strength);
            var targetDefence = targetLivingEntity != null ? GetTargetDefenceValue(targetLivingEntity) : 0;
            var defenceRatio = 100f / (100 + targetDefence);

            var weapon = itemUsed as Weapon;

            if (weapon != null && weapon.IsRanged)
            {
                attackStrength /= weapon.GetAmmoPerSecond();
            }

            //Even a small attack can still do damage
            var damage = Mathf.Ceil(attackStrength * defenceRatio / CombatItemBase.StrengthDivisor);

            if (weapon != null && weapon.IsTwoHanded)
            {
                damage *= 2;
            }

            if (weapon != null && weapon.IsMelee)
            {
                damage *= 2;
            }

            var isCritical = false;

            if (sourceFighter != null)
            {
                var sourceFighterCriticalHitChance = GetSourceCriticalHitChance(sourceFighter);
                var criticalTestValue = UnityEngine.Random.Range(0, 101);
                isCritical = criticalTestValue <= sourceFighterCriticalHitChance;

                if (isCritical)
                {
                    //Debug.Log($"CRITICAL! Chance:{sourceFighterCriticalHitChance}, test:{criticalTestValue}");

                    damage *= 2;
                }
            }

            return new CombatResult
            {
                Change = (int)damage,
                IsCriticalHit = isCritical
            };
        }

        private float GetSourceCriticalHitChance(FighterBase sourceFighter)
        {
            var luckValue = sourceFighter.GetAttributeValue(AttributeAffected.Luck);

            if (luckValue < 20)
            {
                return 0;
            }

            //e.g. 50 luck would mean a 50/5 = 10% chance
            return luckValue / 5f;
        }

        private int GetTargetDefenceValue(LivingEntityBase targetLivingEntity)
        {
            //todo: replace with sum of Strength of items with resistance to Hurt

            var armorItems = _typeRegistry
                .GetRegisteredTypes<IArmor>()
                .Select(x => targetLivingEntity.Inventory.GetItemInSlot<Armor>(x.TypeId.ToString(), false));

            var sum = armorItems
                .Where(x => x != null)
                .Sum(a => a.Attributes.Strength);
            return (int)Math.Floor((float)sum / armorItems.Count());
        }
    }
}
