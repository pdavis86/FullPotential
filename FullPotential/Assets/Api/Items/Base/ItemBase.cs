using System.Collections.Generic;
using System.Linq;
using System.Text;

using FullPotential.Api.Data;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Items.Base
{
    public abstract class ItemBase : IItem, ISaveable
    {
        public string Id { get; set; }

        public string CharacterId { get; set; }

        public IRegisterableType RegistryType { get; set; }

        public string Name { get; set; }

        // todo: make Attributes a Dictionary<AttributeTypeId, int>
        public Dictionary<string, string> Attributes { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public bool IsDirty { get; set; }

        public virtual string GetName(ILocalizer localizer)
        {
            return Name;
        }

        public virtual string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full)
        {
            return levelOfDetail == LevelOfDetail.Full ? Name : null;
        }

        public void AppendToDescription(StringBuilder builder, ILocalizer localizer, int attributeValue, string attributeName, string aliasSegment, string aliasValue, UnitsType? unitsType = null)
        {
            if (attributeValue == 0)
            {
                return;
            }

            var aliasTranslation = localizer.Translate(TranslationType.AttributeAlias, aliasSegment + "." + attributeName);
            var aliasUnits = unitsType == null ? null : localizer.Translate(TranslationType.ItemUnits, unitsType.ToString());
            var originalTranslation = localizer.Translate(TranslationType.Attribute, attributeName);
            builder.Append($"{aliasTranslation}: {aliasValue}{aliasUnits} ({originalTranslation}: {attributeValue})\n");
        }

        public virtual IEnumerable<string> ValidateCraftingAttempt(List<ItemBase> components)
        {
            return Enumerable.Empty<string>();
        }

        public abstract string GetTextForUiComponent(string id, IPlayerFighter playerFighter);
    }
}
