using System.Linq;
using System.Text;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;

namespace FullPotential.Api.Items.Weapons
{
    [System.Serializable]
    public class Weapon : ItemWithHealthBase
    {
        private const string AliasSegmentWeapon = "Weapon";
        private const string AliasSegmentMelee = "MeleeWeapon";
        private const string AliasSegmentRanged = "RangedWeapon";
        private const string AliasSegmentDefensive = "DefensiveWeapon";

        public int Ammo;

        public int GetAmmoMax()
        {
            const int ammoCap = 100;
            var returnValue = (int)(Attributes.Efficiency / 100f * ammoCap);
            //Debug.Log("GetAmmoMax: " + returnValue);
            return returnValue;
        }

        public float GetMeleeRecoveryTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Recovery, 0.5f, 5);
            //Debug.Log("GetMeleeRecoveryTime: " + returnValue);
            return returnValue;
        }

        public float GetReloadTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Recovery, 0.5f, 5);
            //Debug.Log("GetReloadTime: " + returnValue);
            return returnValue;
        }

        public float GetMeleeWindUpTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Speed, 0.05f, 0.5f);
            //Debug.Log("GetMeleeWindUpTime: " + returnValue);
            return returnValue;
        }

        public float GetFireRate()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Speed, 0.05f, 0.5f);
            //Debug.Log("GetWeaponFireRate: " + returnValue);
            return returnValue;
        }

        public int GetDefenseValue()
        {
            return IsTwoHanded
                ? Attributes.Strength * 2
                : Attributes.Strength;
        }

        public float GetMeleeDps()
        {
            //todo: zzz v0.5 - remove this when data is in a database
            if (Attributes.Recovery == 0)
            {
                Attributes.Recovery = 1;
            }

            var damage = _valueCalculator.GetDamageValueFromAttack(this, 0, false);

            var windUp = GetMeleeWindUpTime();
            var timeForTwoAttacks = windUp + GetMeleeRecoveryTime() + windUp;

            return damage * 2 / timeForTwoAttacks;
        }

        public float GetRangedDps()
        {
            var damage = _valueCalculator.GetDamageValueFromAttack(this, 0, false);
            var ammoMax = GetAmmoMax();
            var bulletsPerSecond = 1 / GetFireRate();

            return damage * ammoMax / (ammoMax / bulletsPerSecond + GetReloadTime());
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            if (RegistryType is not IGearWeapon weaponType)
            {
                Debug.LogError($"Unexpected RegistryType on item '{Name}' with ID '{Id}'");
                return null;
            }

            switch (weaponType.Category)
            {
                case IGearWeapon.WeaponCategory.Defensive:
                    return GetDefensiveWeaponDescription(localizer, levelOfDetail, itemName);

                case IGearWeapon.WeaponCategory.Melee:
                    return GetMeleeWeaponDescription(localizer, levelOfDetail, itemName);

                case IGearWeapon.WeaponCategory.Ranged:
                    return GetRangedWeaponDescription(localizer, levelOfDetail, itemName);

                default:
                    Debug.LogError($"Unexpected weaponType.Category on item '{Name}' with ID '{Id}'");
                    return null;
            }
        }

        public string GetDefensiveWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
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

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentDefensive,
                GetDefenseValue().ToString(_gameManager.CurrentCulture));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentWeapon,
                RoundFloatForDisplay(GetMeleeDps()),
                UnitsType.UnitPerTime);

            //todo: what does speed do for a defensive weapon?
            AppendToDescription(sb, localizer, Attributes.Speed, nameof(Attributes.Speed));

            //todo: what does speed do for a defensive weapon?
            AppendToDescription(sb, localizer, Attributes.Recovery, nameof(Attributes.Recovery));

            return sb.ToString();
        }

        public string GetMeleeWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
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

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentWeapon,
                RoundFloatForDisplay(GetMeleeDps()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentMelee,
                RoundFloatForDisplay(GetMeleeWindUpTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                AliasSegmentMelee,
                RoundFloatForDisplay(GetMeleeRecoveryTime()),
                UnitsType.UnitPerTime);

            return sb.ToString();
        }

        public string GetRangedWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
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

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentWeapon,
                RoundFloatForDisplay(GetRangedDps()),
                UnitsType.UnitPerTime);

            if (levelOfDetail >= LevelOfDetail.Intermediate)
            {
                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.Efficiency,
                    nameof(Attributes.Efficiency),
                    AliasSegmentRanged,
                    RoundFloatForDisplay(GetAmmoMax()));
            }

            AppendToDescription(
                sb,
                localizer,
                Attributes.Range,
                nameof(Attributes.Range),
                AliasSegmentWeapon,
                RoundFloatForDisplay(GetRange()));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Accuracy,
                nameof(Attributes.Accuracy),
                AliasSegmentWeapon,
                RoundFloatForDisplay(GetAccuracy()),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentRanged,
                RoundFloatForDisplay(1 / GetFireRate()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                AliasSegmentRanged,
                RoundFloatForDisplay(GetReloadTime()),
                UnitsType.Time);

            return sb.ToString();
        }
    }
}
