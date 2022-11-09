using System.Text;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [System.Serializable]
    public class Accessory : ItemWithHealthBase
    {
        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            //todo: zzz v0.4.1 - implement accessory effects
            //if (Effects != null && Effects.Count > 0)
            //{
            //    var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
            //    sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            //}

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));

            //todo: zzz v0.4.1 - what is accessory strength used for?
            //AppendToDescription(sb, localizer, Attributes.Strength, nameof(Attributes.Strength));
            var originalTranslation = localizer.Translate(TranslationType.Attribute, nameof(Attributes.Strength));
            sb.Append($"WiP ({originalTranslation}: {Attributes.Strength})\n");

            return sb.ToString();
        }
    }
}
