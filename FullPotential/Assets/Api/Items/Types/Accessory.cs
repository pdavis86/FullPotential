using System;
using System.Text;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class Accessory : ItemWithHealthBase, IHasVisuals
    {
        private IVisuals _visuals;

        //Variables so they are serialized
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable NotAccessedField.Global
        public string AccessoryVisualsTypeId;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore NotAccessedField.Global

        public string VisualsTypeId => AccessoryVisualsTypeId;

        public IVisuals Visuals
        {
            get => _visuals;
            set
            {
                _visuals = value;
                AccessoryVisualsTypeId = _visuals?.TypeId.ToString();
            }
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
            }

            //todo: zzz v0.4 - implement accessory triggers and traits
            //if (Effects != null && Effects.Count > 0)
            //{
            //    var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
            //    sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            //}

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));

            //todo: zzz v0.4 - implement accessory triggers and traits
            //sb.Append($"WiP ({localizer.Translate(TranslationType.Attribute, nameof(Attributes.Strength))}: {Attributes.Strength})\n");
            //sb.Append($"WiP ({localizer.Translate(TranslationType.Attribute, nameof(Attributes.Efficiency))}: {Attributes.Efficiency})\n");
            //sb.Append($"WiP ({localizer.Translate(TranslationType.Attribute, nameof(Attributes.Speed))}: {Attributes.Speed})\n");
            //sb.Append($"WiP ({localizer.Translate(TranslationType.Attribute, nameof(Attributes.Recovery))}: {Attributes.Recovery})\n");

            return sb.ToString().Trim();
        }

        public static string GetSlotId(string typeId, int index)
        {
            return $"{typeId};{index}";
        }
    }
}
