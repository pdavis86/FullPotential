using System;

using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Gameplay.Combat
{
    public class CombatCalculator
    {
        private readonly Random _random;

        public CombatCalculator()
            : this(new Random())
        {
        }

        public CombatCalculator(Random random)
        {
            _random = random;
        }

        public float GetEffectivePercentage(int targetDefence)
        {
            return 100f / (100 + targetDefence);
        }

        public float GetCriticalHitChance(float luckValue)
        {
            return luckValue < 20 ? 0 : luckValue / 5f;
        }

        public float GetEffectBaseChange(
            float baseValue,
            bool isWeapon,
            bool isMeleeWeapon,
            float ammoPerSecond,
            bool isTwoHanded,
            bool isCriticalHit)
        {
            var change = baseValue;

            if (isWeapon)
            {
                change = isMeleeWeapon ? change * 2 : change / ammoPerSecond;
            }

            if (isTwoHanded)
            {
                change *= 2;
            }

            if (isCriticalHit)
            {
                change *= 2;
            }

            return change;
        }

        public CombatResult GetCombatResult(
            float criticalHitChance,
            float baseChange,
            float effectivePercentage,
            IEffectType effect)
        {
            var isCriticalHit = criticalHitChance >= 0 && _random.Next(0, 101) <= criticalHitChance;
            var change = isCriticalHit ? baseChange * 2 : baseChange;
            var adjustedChange = AddVariationToValue(change * effectivePercentage);

            switch (effect)
            {
                case IValuePoolEffectType valuePoolEffect:
                    if (valuePoolEffect.EffectActionType is EffectActionType.PeriodicDecrease
                        or EffectActionType.SingleDecrease
                        or EffectActionType.TemporaryMaxDecrease)
                    {
                        adjustedChange *= -1;
                    }

                    break;
                case IAttributeEffect attributeEffect when !attributeEffect.IsTemporaryMaxIncrease:
                    adjustedChange *= -1;
                    break;
            }

            return new CombatResult
            {
                Change = adjustedChange,
                IsCriticalHit = isCriticalHit
            };
        }

        private int AddVariationToValue(float basicValue)
        {
            var between90And110Percent = (int)Math.Ceiling(basicValue * _random.Next(90, 111) / 100);
            var tenPercent = (int)Math.Ceiling(basicValue / 10f);
            var adder = _random.Next(0, Math.Min(tenPercent, 6));

            return basicValue < 0
                ? between90And110Percent - adder
                : between90And110Percent + adder;
        }
    }
}
