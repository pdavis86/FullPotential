using System.Linq;
using System.Text;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.SpellsAndGadgets
{
    [System.Serializable]
    public class Spell : SpellOrGadgetItemBase
    {
        public Spell()
        {
            ResourceConsumptionType = ResourceConsumptionType.Mana;
        }

        //todo: add mana consumption
        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            if (Targeting != null) { sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Targeting))}: {localizer.GetTranslatedTypeName(Targeting)}\n"); }
            if (Shape != null) { sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Shape))}: {localizer.GetTranslatedTypeName(Shape)}\n"); }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));
            AppendToDescription(sb, localizer, Attributes.Strength, nameof(Attributes.Strength));
            AppendToDescription(sb, localizer, Attributes.Efficiency, nameof(Attributes.Efficiency));
            AppendToDescription(sb, localizer, Attributes.Range, nameof(Attributes.Range));
            AppendToDescription(sb, localizer, Attributes.Accuracy, nameof(Attributes.Accuracy));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                nameof(Spell),
                RoundFloatForDisplay(GetChargeTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                nameof(Spell),
                RoundFloatForDisplay(GetCooldownTime()),
                UnitsType.Time);

            AppendToDescription(sb, localizer, Attributes.Duration, nameof(Attributes.Duration));

            return sb.ToString();
        }
    }
}
