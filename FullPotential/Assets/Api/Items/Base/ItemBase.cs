using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using Unity.Mathematics;

namespace FullPotential.Api.Items.Base
{
    [Serializable]
    public abstract class ItemBase
    {
        public const string AliasSegmentItem = "item";

        // ReSharper disable InconsistentNaming
        protected IGameManager _gameManager;
        protected IValueCalculator _valueCalculator;
        // ReSharper restore InconsistentNaming

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

        protected ItemBase()
        {
            _gameManager = ModHelper.GetGameManager();
            _valueCalculator = _gameManager.GetService<IValueCalculator>();
        }

        //public float GetHighInHighOutInRange(int attributeValue, float min, float max)
        //{
        //    return attributeValue / 100f * (max - min) + min;
        //}

        public float GetHighInLowOutInRange(int attributeValue, float min, float max)
        {
            return (101 - attributeValue) / 100f * (max - min) + min;
        }

        public string RoundFloatForDisplay(float input, int decimalPlaces = 1)
        {
            var rounded = Math.Round(input, decimalPlaces);
            return rounded.ToString(ModHelper.GetGameManager().CurrentCulture);
        }

        public int AddVariationToValue(double basicValue)
        {
            var multiplier = ValueCalculator.Random.Next(90, 111) / 100f;
            var adder = ValueCalculator.Random.Next(0, 6);
            return (int)Math.Ceiling(basicValue / multiplier) + adder;
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

        public virtual string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));
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

            return sb.ToString();
        }

        public float GetMovementForceValue(bool adjustForGravity)
        {
            var force = 4f * Attributes.Strength;

            return adjustForGravity
                ? force * 1.2f
                : force;
        }

        public virtual float GetRange()
        {
            var returnValue = Attributes.Range / 100f * 15 + 5;
            //Debug.Log("GetProjectileRange: " + returnValue);
            return returnValue;
        }

        public virtual float GetAccuracy()
        {
            var returnValue = Attributes.Accuracy;
            //Debug.Log("GetAccuracy: " + returnValue);
            return returnValue;
        }

        public float GetEffectTimeBetween(float min = 0.5f, float max = 1.5f)
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Speed, min, max);
            //Debug.Log("GetTimeBetweenEffects: " + returnValue);
            return returnValue;
        }

        public (int Change, DateTime Expiry) GetStatChangeAndExpiry(IStatEffect statEffect)
        {
            var change = AddVariationToValue(Attributes.Strength / 5f);

            if (statEffect.Affect is Affect.PeriodicDecrease or Affect.SingleDecrease or Affect.TemporaryMaxDecrease)
            {
                change *= -1;
            }

            var timeToLive = statEffect.Affect == Affect.SingleIncrease || statEffect.Affect == Affect.SingleDecrease
                ? math.ceil(Attributes.Duration / 50f)
                : math.ceil(Attributes.Duration / 20f);

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }

        public (int Change, DateTime Expiry, float delay) GetPeriodicStatChangeExpiryAndDelay(IStatEffect statEffect)
        {
            var (change, expiry) = GetStatChangeAndExpiry(statEffect);

            var delay = GetHighInLowOutInRange(Attributes.Recovery, 0.5f, 3);

            return (change, expiry, delay);
        }

        public (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(IAttributeEffect attributeEffect)
        {
            var change = AddVariationToValue(Attributes.Strength / 5f);

            if (!attributeEffect.TemporaryMaxIncrease)
            {
                change *= -1;
            }

            var timeToLive = Attributes.Duration / 2f;

            return (change, DateTime.Now.AddSeconds(timeToLive));
        }

    }
}
