using System.Linq;
using System.Text;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Weapons
{
    [System.Serializable]
    public class RangedWeapon : WeaponItemBase
    {
        //todo: check each displayed prop has been implemented as a restriction, trait etc.
        //todo: add DPS



        public override string GetDescription(ILocalizer localizer, bool showExtendedDetails = true, string itemName = null)
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
                    nameof(WeaponItemBase),
                    RoundFloatForDisplay(GetAmmoMax()));
            }

            AppendToDescription(sb, localizer, Attributes.Range, nameof(Attributes.Range));
            AppendToDescription(sb, localizer, Attributes.Accuracy, nameof(Attributes.Accuracy));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                nameof(WeaponItemBase),
                RoundFloatForDisplay(1 / GetFireRate(), 2),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                nameof(WeaponItemBase),
                RoundFloatForDisplay(GetReloadTime()),
                UnitsType.Time);

            return sb.ToString();
        }
    }
}
