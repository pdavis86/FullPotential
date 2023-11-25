using System;
using System.Text;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class SpecialGear : ItemWithHealthBase, IResourceConsumer
    {
        private IResource _resourceType;

        public string ResourceTypeId;

        public IResource ResourceType
        {
            get => _resourceType;
            set
            {
                _resourceType = value;
                ResourceTypeId = _resourceType?.TypeId.ToString();
            }
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            if (ResourceType != null)
            {
                var resourceConsumptionType = localizer.Translate(ResourceType);

                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.Efficiency,
                    nameof(Attributes.Efficiency),
                    Consumer.AliasSegmentConsumer,
                    $"{GetResourceCost()} {resourceConsumptionType}");
            }

            return sb.ToString().Trim();
        }
    }
}
