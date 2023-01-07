using System.Linq;
using System.Text;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;

namespace FullPotential.Api.Items.Types
{
    [System.Serializable]
    public class Weapon : ItemWithHealthBase
    {
        private const string AliasSegmentMelee = "MeleeWeapon";
        private const string AliasSegmentRanged = "RangedWeapon";

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
            var returnValue = GetHighInLowOutInRange(Attributes.Recovery, 0.5f, 5);
            //Debug.Log("GetMeleeRecoveryTime: " + returnValue);
            return returnValue;
        }

        public float GetReloadTime()
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Recovery, 0.5f, 5);
            //Debug.Log("GetReloadTime: " + returnValue);
            return returnValue;
        }

        public float GetMeleeWindUpTime()
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Speed, 0.05f, 0.5f);
            //Debug.Log("GetMeleeWindUpTime: " + returnValue);
            return returnValue;
        }

        public float GetDelayBetweenShots()
        {
            //Roughly 2-6 bullets per second
            var returnValue = GetHighInLowOutInRange(Attributes.Speed, 0.07f, 0.4f);
            return returnValue;
        }

        public float GetBulletsPerSecond()
        {
            return 1 / GetDelayBetweenShots();
        }

        public int GetDefenseValue()
        {
            return IsTwoHanded
                ? Attributes.Strength * 2
                : Attributes.Strength;
        }

        public float GetMeleeDps()
        {
            var damage = _valueCalculator.GetDamageValueFromAttack(this, 0, false);

            var windUp = GetMeleeWindUpTime();
            var timeForTwoAttacks = windUp + GetMeleeRecoveryTime() + windUp;

            return damage * 2 / timeForTwoAttacks;
        }

        public float GetRangedDps()
        {
            var damage = _valueCalculator.GetDamageValueFromAttack(this, 0, false);
            return GetDamagePerSecond(damage, GetAmmoMax(), GetBulletsPerSecond(), GetReloadTime());
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

            //todo: zzz v0.4.1 - what does Speed do for a defensive weapon?
            //AppendToDescription(sb, localizer, Attributes.Speed, nameof(Attributes.Speed));

            //todo: zzz v0.4.1 - what does Recovery do for a defensive weapon?
            //AppendToDescription(sb, localizer, Attributes.Recovery, nameof(Attributes.Recovery));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentDefensive,
                GetDefenseValue().ToString(_gameManager.CurrentCulture));

            //todo: zzz v0.4.1 - Implement melee with a sheild
            //AppendToDescription(
            //    sb,
            //    localizer,
            //    Attributes.Strength,
            //    nameof(Attributes.Strength),
            //    AliasSegmentItem,
            //    RoundFloatForDisplay(GetMeleeDps()),
            //    UnitsType.UnitPerTime);

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
                Attributes.Efficiency,
                nameof(Attributes.Efficiency),
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

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentItem,
                RoundFloatForDisplay(GetMeleeDps()),
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

            //All ranged weapons have maximum range
            //AppendToDescription(
            //    sb,
            //    localizer,
            //    Attributes.Range,
            //    nameof(Attributes.Range),
            //    AliasSegmentItem,
            //    RoundFloatForDisplay(GetRange()));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Accuracy,
                nameof(Attributes.Accuracy),
                AliasSegmentItem,
                RoundFloatForDisplay(GetAccuracy()),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentRanged,
                RoundFloatForDisplay(GetBulletsPerSecond()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                AliasSegmentRanged,
                RoundFloatForDisplay(GetReloadTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentItem,
                RoundFloatForDisplay(GetRangedDps()),
                UnitsType.UnitPerTime);

            return sb.ToString();
        }
    }
}
