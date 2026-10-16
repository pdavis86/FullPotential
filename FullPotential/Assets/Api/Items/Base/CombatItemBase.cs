using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;

using UnityEngine;

namespace FullPotential.Api.Items.Base
{
    public abstract class CombatItemBase : ItemBase, IHasEffects
    {
        public const float DefensiveWeaponDpsMultiplier = 0.1f;
        public const string AliasSegmentItem = "item";
        public const string AliasSegmentDefensive = "defensiveitem";

        private readonly ICombatService _combatService;
        private readonly ITypeRegistry _typeRegistry;


        private int _baseDamage = -1;
        private float _meleeDps = -1;
        private float _defensiveDps = -1;
        private float _rangedDps = -1;
        private IEffectType _hurtEffect;

        public bool IsTwoHanded
        {
            get => Properties.GetBoolValue(nameof(IsTwoHanded));
            set => Properties[nameof(IsTwoHanded)] = value.ToString();
        }

        public string ShapeCode
        {
            get => Properties.GetStringValueOrNull(nameof(ShapeCode));
            set => Properties[nameof(ShapeCode)] = value;
        }

        //public List<string> EffectIds { get; set; }

        public List<IEffectType> EffectTypes { get; set; }

        protected CombatItemBase()
        {
            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
        }

        public int GetNameHash()
        {
            var hash = 101;
            hash = hash * 103 + Id.GetHashCode();
            hash = hash * 107 + (RegistryType.TypeId.ToString() ?? string.Empty).GetHashCode();
            hash = hash * 109 + (Name ?? string.Empty).GetHashCode();
            foreach (var attribute in Attributes.OrderBy(x => x.Key))
            {
                hash = hash * 113 + attribute.Key.GetHashCode();
                hash = hash * 127 + attribute.Value.GetHashCode();
            }
            hash = hash * 127 + (EffectTypes != null ? string.Join(null, EffectTypes) : string.Empty).GetHashCode();
            return hash;
        }

        public void AppendToDescription(StringBuilder builder, ILocalizer localizer, bool attributeValue, string attributeName)
        {
            if (attributeValue)
            {
                builder.Append(localizer.Translate(TranslationType.Attribute, attributeName) + "\n");
            }
        }

        public void AppendToDescription(StringBuilder builder, ILocalizer localizer, int attributeValue, string attributeName)
        {
            if (attributeValue > 0)
            {
                builder.Append($"{localizer.Translate(TranslationType.Attribute, attributeName)}: {attributeValue}\n");
            }
        }

        public float GetEffectDuration()
        {
            var returnValue = Attributes.GetIntValue(Duration.TypeIdString) / 10f;
            return returnValue;
        }

        public float GetChargeUpTime()
        {
            return GetChargeUpOrCooldownTime(Attributes.GetIntValue(Speed.TypeIdString));
        }

        public float GetCooldownTime()
        {
            return GetChargeUpOrCooldownTime(Attributes.GetIntValue(Recovery.TypeIdString));
        }

        public float GetDamagePerSecond(float damagePerItem, int numberOfItems, float itemsPerSecond, float reloadTime)
        {
            //https://www.reddit.com/r/Overwatch/comments/6x5q59/what_dps_is_and_how_to_calculate_it/

            return damagePerItem * numberOfItems / ((numberOfItems / itemsPerSecond) + reloadTime);
        }

        public int GetResourceCost()
        {
            var returnValue = MathsHelper.GetHighInLowOutInRange(Attributes.GetIntValue(Efficiency.TypeIdString), 5, 50);

            //todo: zzz v0.8 - trait-based mana cost

            return (int)returnValue * Math.Max(EffectTypes?.Count ?? 1, 1);
        }

        public int GetSingleEffectValueChange()
        {
            return LivingEntityBase.GetAdjustedStrength(Attributes.GetIntValue(Strength.TypeIdString));
        }

        public int GetPeriodicEffectValueChange()
        {
            var effectDuration = GetEffectDuration();
            var timeBetweenEffects = GetChargeUpTime();
            var maxNumberOfTimes = (int)Mathf.Ceil(effectDuration / timeBetweenEffects);
            var minNumberOfTimes = (int)Mathf.Floor(effectDuration / timeBetweenEffects);
            var effectsPerSecond = 1 / timeBetweenEffects;

            if (minNumberOfTimes == 0)
            {
                minNumberOfTimes = 1;
            }

            if (maxNumberOfTimes == 0)
            {
                maxNumberOfTimes = 1;
            }

            var changeOverTimeFloat = GetDamagePerSecond((float)GetSingleEffectValueChange() / maxNumberOfTimes, minNumberOfTimes, effectsPerSecond, 0);

            var changeOverTimeSignedInt = Math.Sign(changeOverTimeFloat) * (int)Mathf.Ceil(Mathf.Abs(changeOverTimeFloat));
            return changeOverTimeSignedInt;
        }

        private float GetChargeUpOrCooldownTime(int attributeValue)
        {
            var returnValue = MathsHelper.GetHighInLowOutInRange(attributeValue, 0.05f, 2f);
            return returnValue;
        }

        public int GetAmmoMax()
        {
            const int ammoCap = 100;
            var returnValue = (int)(Attributes.GetIntValue(Efficiency.TypeIdString) / 100f * ammoCap);
            return returnValue;
        }

        public float GetReloadTime()
        {
            var returnValue = MathsHelper.GetHighInLowOutInRange(Attributes.GetIntValue(Recovery.TypeIdString), 0.5f, 5);
            return returnValue;
        }

        public float GetDelayBetweenShots()
        {
            //Roughly 2-6 bullets per second
            var returnValue = MathsHelper.GetHighInLowOutInRange(Attributes.GetIntValue(Speed.TypeIdString), 0.07f, 0.4f);
            return returnValue;
        }

        public float GetAmmoPerSecond()
        {
            return 1 / GetDelayBetweenShots();
        }

        public float GetMeleeDps()
        {
            if (_meleeDps >= 0)
            {
                return _meleeDps;
            }

            var windUp = GetChargeUpTime();
            var timeForTwoAttacks = windUp + GetCooldownTime() + windUp;

            _meleeDps = GetBaseDamage() * 2 / timeForTwoAttacks;

            return _meleeDps;
        }

        public float GetDefensiveDps()
        {
            if (_defensiveDps >= 0)
            {
                return _defensiveDps;
            }

            var windUp = GetChargeUpTime();
            var timeForTwoAttacks = windUp + GetCooldownTime() + windUp;

            _defensiveDps = GetBaseDamage() * 2 / timeForTwoAttacks * DefensiveWeaponDpsMultiplier;

            return _defensiveDps;
        }

        public float GetRangedDps()
        {
            if (_rangedDps >= 0)
            {
                return _rangedDps;
            }

            _rangedDps = GetDamagePerSecond(GetBaseDamage(), GetAmmoMax(), GetAmmoPerSecond(), GetReloadTime());

            return _rangedDps;
        }

        private int GetBaseDamage()
        {
            if (_baseDamage >= 0)
            {
                return _baseDamage;
            }

            var baseDamage = 0;

            if (!EffectTypes.Any())
            {
                _hurtEffect ??= _typeRegistry.GetRegisteredByTypeId<IEffectType>(EffectTypeIds.HurtId);
                baseDamage = (int)_combatService.GetEffectBaseChange(null, this, _hurtEffect, false);
            }
            else
            {
                foreach (var effect in EffectTypes)
                {
                    baseDamage += (int)_combatService.GetEffectBaseChange(null, this, effect, false);
                }
            }

            return _baseDamage = baseDamage;
        }
    }
}
