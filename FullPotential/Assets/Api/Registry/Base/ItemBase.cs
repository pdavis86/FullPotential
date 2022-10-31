﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;

namespace FullPotential.Api.Registry.Base
{
    [Serializable]
    public abstract class ItemBase
    {
        public string Id;
        public string RegistryTypeId;
        public string Name;
        public Attributes Attributes;
        public string[] EffectIds;
        public bool IsTwoHanded;

        private List<IEffect> _effects;
        public List<IEffect> Effects
        {
            get
            {
                return _effects;
            }
            set
            {
                _effects = value;
                EffectIds = _effects.Select(x => x.TypeId.ToString()).ToArray();
            }
        }

        private IRegisterable _registryType;
        public IRegisterable RegistryType
        {
            get
            {
                return _registryType;
            }
            set
            {
                _registryType = value;
                RegistryTypeId = _registryType.TypeId.ToString();
            }
        }

        public static float GetValueInRange(int attributeValue, float min, float max)
        {
            return attributeValue / 100f * (max - min) + min;
        }

        public static float GetValueInRangeHighLow(int attributeValue, float min, float max)
        {
            return (101 - attributeValue) / 100f * (max - min) + min;
        }

        public static string RoundFloatForDisplay(float input, int decimalPlaces = 1)
        {
            var rounded = Math.Round(input, decimalPlaces);
            return rounded.ToString(GameManager.Instance.CurrentCulture);
        }

        public int GetNameHash()
        {
            unchecked
            {
                var hash = 101;
                hash = hash * 103 + Id.GetHashCode();
                hash = hash * 107 + (RegistryTypeId ?? string.Empty).GetHashCode();
                hash = hash * 109 + (Name ?? string.Empty).GetHashCode();
                hash = hash * 113 + Attributes.GetHashCode();
                hash = hash * 127 + (EffectIds != null ? string.Join(null, EffectIds) : string.Empty).GetHashCode();
                return hash;
            }
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

        protected void AppendToDescription(StringBuilder builder, ILocalizer localizer, int attributeValue, string attributeName, string aliasSegment, string aliasValue, UnitsType? unitsType = null)
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

        public virtual string GetDescription(ILocalizer localizer, bool showExtendedDetails = true, string itemName = null)
        {
            var sb = new StringBuilder();

            if (showExtendedDetails)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, Attributes.IsAutomatic, nameof(Attributes.IsAutomatic));
            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));
            AppendToDescription(sb, localizer, Attributes.ExtraAmmoPerShot, nameof(Attributes.ExtraAmmoPerShot));

            AppendToDescription(sb, localizer, Attributes.Strength, nameof(Attributes.Strength));
            AppendToDescription(sb, localizer, Attributes.Efficiency, nameof(Attributes.Efficiency));
            AppendToDescription(sb, localizer, Attributes.Range, nameof(Attributes.Range));
            AppendToDescription(sb, localizer, Attributes.Accuracy, nameof(Attributes.Accuracy));
            AppendToDescription(sb, localizer, Attributes.Speed, nameof(Attributes.Speed));
            AppendToDescription(sb, localizer, Attributes.Recovery, nameof(Attributes.Recovery));
            AppendToDescription(sb, localizer, Attributes.Duration, nameof(Attributes.Duration));
            AppendToDescription(sb, localizer, Attributes.Luck, nameof(Attributes.Luck));

            return sb.ToString();
        }

    }
}
