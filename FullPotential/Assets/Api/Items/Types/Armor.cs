using System;
using System.Text;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class Armor : ItemWithHealthBase, IDefensible
    {
        public int GetDefenseValue()
        {
            return Attributes.Strength;
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            return GetArmorCategory() == ArmorCategory.Barrier
                ? GetBarrierDescription(localizer, levelOfDetail, itemName)
                : GetArmorDescription(localizer, levelOfDetail, itemName);
        }

        private ArmorCategory GetArmorCategory()
        {
            if (RegistryType is not IGearArmor armor)
            {
                throw new Exception("Registry type was not IGearArmor");
            }

            return armor.Category;
        }

        private float GetRechargeDelay()
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Speed, 0.5f, 1.5f);
            //Debug.Log("GetRechargeDelay: " + returnValue);
            return returnValue;
        }

        private float GetRechargeRate()
        {
            var returnValue = GetHighInHighOutInRange(Attributes.Recovery, 0.5f, 1.5f);
            //Debug.Log("GetRechargeRate: " + returnValue);
            return returnValue;
        }

        private string GetArmorDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            //todo: zzz v0.4.1 - implement armor effects
            //if (Effects != null && Effects.Count > 0)
            //{
            //    var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
            //    sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            //}

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));
            AppendToDescription(sb, localizer, Attributes.IsAutomatic, nameof(Attributes.IsAutomatic));
            AppendToDescription(sb, localizer, Attributes.ExtraAmmoPerShot, nameof(Attributes.ExtraAmmoPerShot));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentDefensive,
                GetDefenseValue().ToString(_gameManager.CurrentCulture));

            return sb.ToString();
        }

        private string GetBarrierDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            //todo: zzz v0.4.1 - implement armor effects
            //if (Effects != null && Effects.Count > 0)
            //{
            //    var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
            //    sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            //}

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));

            AppendToDescription(
               sb,
               localizer,
               Attributes.Strength,
               nameof(Attributes.Strength),
               AliasSegmentDefensive,
               GetDefenseValue().ToString(_gameManager.CurrentCulture));

            //What does Efficiency do for a barrier?
            //AppendToDescription(sb, localizer, Attributes.Efficiency, nameof(Attributes.Efficiency));

            //todo: zzz v0.4.1 - implement borderlands-like shields
            //AppendToDescription(
            //    sb,
            //    localizer,
            //    Attributes.Speed,
            //    nameof(Attributes.Speed),
            //    nameof(IGearArmor.ArmorCategory.Barrier),
            //    RoundFloatForDisplay(GetRechargeDelay()),
            //    UnitsType.Time);

            //todo: zzz v0.4.1 - implement borderlands-like shields
            //AppendToDescription(
            //    sb,
            //    localizer,
            //    Attributes.Recovery,
            //    nameof(Attributes.Recovery),
            //    nameof(IGearArmor.ArmorCategory.Barrier),
            //    RoundFloatForDisplay(GetRechargeRate()),
            //    UnitsType.UnitPerTime);

            return sb.ToString();
        }
    }
}
