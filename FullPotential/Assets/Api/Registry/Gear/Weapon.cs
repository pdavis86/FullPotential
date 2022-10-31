using System.Linq;
using System.Text;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;

namespace FullPotential.Api.Registry.Gear
{
    [System.Serializable]
    public class Weapon : GearBase
    {
        public int Ammo;

        public float GetReloadTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Recovery, 0.5f, 5);
            //Debug.Log("GetReloadTime: " + returnValue);
            return returnValue;
        }

        public int GetAmmoMax()
        {
            var ammoCap = Attributes.IsAutomatic ? 100 : 20;
            var returnValue = (int)(Attributes.Efficiency / 100f * ammoCap);
            //Debug.Log("GetAmmoMax: " + returnValue);
            return returnValue;
        }

        public float GetFireRate()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Speed, 0.5f, 3f);
            //Debug.Log("GetWeaponFireRate: " + returnValue);
            return returnValue;
        }

        public float GetRange()
        {
            var returnValue = Attributes.Range / 100f * 15 + 15;
            //Debug.Log("GetProjectileRange: " + returnValue);
            return returnValue;
        }


        public override string GetDescription(ILocalizer localizer, bool showExtendedDetails = true, string itemName = null)
        {
            if (RegistryType is not IGearWeapon weaponType)
            {
                Debug.LogError($"Unexpected RegistryType on item '{Name}' with ID '{Id}'");
                return null;
            }

            switch (weaponType.Category)
            {
                case IGearWeapon.WeaponCategory.Ranged:
                    return GetRangedWeaponDescription(localizer, showExtendedDetails, itemName);

                case IGearWeapon.WeaponCategory.Defensive:
                    return GetDefensiveWeaponDescription(localizer, showExtendedDetails, itemName);

                case IGearWeapon.WeaponCategory.Melee:
                    return base.GetDescription(localizer, showExtendedDetails, itemName);

                default:
                    Debug.LogError($"Unexpected weaponType.Category on item '{Name}' with ID '{Id}'");
                    return null;
            }
        }

        private string GetRangedWeaponDescription(ILocalizer localizer, bool showExtendedDetails, string itemName)
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

            //todo: Don't know what to call this yet
            AppendToDescription(sb, localizer, Attributes.Strength, nameof(Attributes.Strength));

            if (showExtendedDetails)
            {
                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.Efficiency,
                    nameof(Attributes.Efficiency),
                    nameof(Weapon),
                    RoundFloatForDisplay(GetAmmoMax()));
            }

            AppendToDescription(sb, localizer, Attributes.Range, nameof(Attributes.Range));
            AppendToDescription(sb, localizer, Attributes.Accuracy, nameof(Attributes.Accuracy));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                nameof(Weapon),
                RoundFloatForDisplay(GetFireRate()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                nameof(Weapon),
                RoundFloatForDisplay(GetReloadTime()),
                UnitsType.Time);

            return sb.ToString();
        }

        private string GetDefensiveWeaponDescription(ILocalizer localizer, bool showExtendedDetails, string itemName)
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

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));
            AppendToDescription(sb, localizer, Attributes.Strength, nameof(Attributes.Strength));
            AppendToDescription(sb, localizer, Attributes.Speed, nameof(Attributes.Speed));
            AppendToDescription(sb, localizer, Attributes.Recovery, nameof(Attributes.Recovery));

            return sb.ToString();
        }

    }
}
