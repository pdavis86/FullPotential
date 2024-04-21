using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;

namespace FullPotential.Api.Items.Base
{
    [Serializable]
    public abstract class CombatItemBase : ItemBase
    {
        public const string AliasSegmentItem = "item";
        public const string AliasSegmentDefensive = "defensiveitem";

        private const float MaximumAccuracyAngleDeviation = 4;

        public Attributes Attributes;
        public string[] EffectIds;
        public bool IsTwoHanded;

        private List<IEffectType> _effects;
        public List<IEffectType> Effects
        {
            get
            {
                return _effects;
            }
            set
            {
                _effects = value;
                EffectIds = _effects
                    .Select(x => x.TypeId.ToString())
                    .ToArray();
            }
        }

        public int GetNameHash()
        {
            var hash = 101;
            hash = hash * 103 + Id.GetHashCode();
            hash = hash * 107 + (RegistryTypeId ?? string.Empty).GetHashCode();
            hash = hash * 109 + (Name ?? string.Empty).GetHashCode();
            hash = hash * 113 + Attributes.GetHashCode();
            hash = hash * 127 + (EffectIds != null ? string.Join(null, EffectIds) : string.Empty).GetHashCode();
            return hash;
        }

        protected void AppendToDescription(StringBuilder builder, ILocalizer localizer, bool attributeValue, string attributeName)
        {
            if (attributeValue)
            {
                builder.Append(localizer.Translate(TranslationType.Attribute, attributeName) + "\n");
            }
        }

        protected void AppendToDescription(StringBuilder builder, ILocalizer localizer, int attributeValue, string attributeName)
        {
            if (attributeValue > 0)
            {
                builder.Append($"{localizer.Translate(TranslationType.Attribute, attributeName)}: {attributeValue}\n");
            }
        }

        public void AppendToDescription(StringBuilder builder, ILocalizer localizer, int attributeValue, string attributeName, string aliasSegment, string aliasValue, UnitsType? unitsType = null)
        {
            if (attributeValue == 0)
            {
                return;
            }

            var aliasTranslation = localizer.Translate(TranslationType.AttributeAlias, aliasSegment + "." + attributeName);
            var aliasUnits = unitsType == null ? null : localizer.Translate(TranslationType.AttributeUnits, unitsType.ToString());
            var originalTranslation = localizer.Translate(TranslationType.Attribute, attributeName);
            builder.Append($"{aliasTranslation}: {aliasValue}{aliasUnits} ({originalTranslation}: {attributeValue})\n");
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
            }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, Attributes.IsAutomatic, nameof(Attributes.IsAutomatic));
            AppendToDescription(sb, localizer, Attributes.ExtraAmmoPerShot, nameof(Attributes.ExtraAmmoPerShot));

            AppendToDescription(sb, localizer, Attributes.Strength, nameof(Attributes.Strength));
            AppendToDescription(sb, localizer, Attributes.Efficiency, nameof(Attributes.Efficiency));
            AppendToDescription(sb, localizer, Attributes.Range, nameof(Attributes.Range));
            AppendToDescription(sb, localizer, Attributes.Accuracy, nameof(Attributes.Accuracy));
            AppendToDescription(sb, localizer, Attributes.Speed, nameof(Attributes.Speed));
            AppendToDescription(sb, localizer, Attributes.Recovery, nameof(Attributes.Recovery));
            AppendToDescription(sb, localizer, Attributes.Duration, nameof(Attributes.Duration));
            AppendToDescription(sb, localizer, Attributes.Luck, nameof(Attributes.Luck));

            return sb.ToString().Trim();
        }

        public float GetEffectDuration()
        {
            var returnValue = Attributes.Duration / 10f;
            return returnValue;
        }

        public float GetChargeUpTime()
        {
            return GetChargeUpOrCooldownTime(Attributes.Speed);
        }

        public float GetCooldownTime()
        {
            return GetChargeUpOrCooldownTime(Attributes.Recovery);
        }

        public float GetDamagePerSecond(float damagePerItem, int numberOfItems, float itemsPerSecond, float reloadTime)
        {
            //https://www.reddit.com/r/Overwatch/comments/6x5q59/what_dps_is_and_how_to_calculate_it/

            return (damagePerItem * numberOfItems) / (numberOfItems / itemsPerSecond + reloadTime);
        }

        public Vector3 GetShotDirection(Vector3 aimDirection)
        {
            var maxAccuracyAngleDeviation = (101 - Attributes.Accuracy) / 100f * MaximumAccuracyAngleDeviation;
            return aimDirection.AddNoiseOnAngle(-maxAccuracyAngleDeviation, maxAccuracyAngleDeviation);
        }

        public int GetResourceCost()
        {
            var returnValue = MathsHelper.GetHighInLowOutInRange(Attributes.Efficiency, 5, 50);

            //todo: zzz v0.8 - trait-based mana cost

            return (int)returnValue * Math.Max(Effects?.Count ?? 1, 1);
        }

        public int GetSingleEffectValueChange()
        {
            return LivingEntityBase.GetAdjustedStrength(Attributes.Strength);
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

            var changeOverTimeFloat = GetDamagePerSecond((float)GetSingleEffectValueChange() / maxNumberOfTimes, minNumberOfTimes, effectsPerSecond, 0);

            var changeOverTimeSignedInt = Math.Sign(changeOverTimeFloat) * (int)Mathf.Ceil(Mathf.Abs(changeOverTimeFloat));
            return changeOverTimeSignedInt;
        }

        private float GetChargeUpOrCooldownTime(int attributeValue)
        {
            var returnValue = MathsHelper.GetHighInLowOutInRange(attributeValue, 0.05f, 2f);
            return returnValue;
        }
    }
}
