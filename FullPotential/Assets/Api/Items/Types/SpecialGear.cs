using System;
using System.Text;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Gear;
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

        //public SerializableKeyValuePair<string, string>[] CustomData;

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var specialType = (ISpecialGear) RegistryType;
            var descriptionOverride = specialType.OverrideItemDescription(this, localizer, levelOfDetail);

            if (!descriptionOverride.IsNullOrWhiteSpace())
            {
                return descriptionOverride;
            }

            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
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

        //public void SetCustomData(string key, string value)
        //{
        //    int i;
        //    for (i = 0; i < CustomData.Length; i++)
        //    {
        //        if (CustomData[i].Key != key)
        //        {
        //            continue;
        //        }

        //        CustomData[i].Value = value;
        //        break;
        //    }
        //}

        //public string GetCustomData(string key)
        //{
        //    return CustomData.FirstOrDefault(kvp => kvp.Key == key).Value;
        //}
    }
}
