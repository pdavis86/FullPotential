using System;
using System.Text;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class Armor : ItemWithHealthBase, IDefensible, IHasVisuals
    {
        private IVisuals _visuals;

        //Variables so they are serialized
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable NotAccessedField.Global
        public string ArmorVisualsTypeId;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore NotAccessedField.Global

        public string VisualsTypeId => ArmorVisualsTypeId;

        public IVisuals Visuals
        {
            get => _visuals;
            set
            {
                _visuals = value;
                ArmorVisualsTypeId = _visuals?.TypeId.ToString();
            }
        }

        public int GetDefenseValue()
        {
            return Attributes.Strength;
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
            }

            //todo: zzz v0.4 - implement armor triggers and traits
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
                localizer.TranslateInt(GetDefenseValue()));

            return sb.ToString().Trim();
        }
    }
}
