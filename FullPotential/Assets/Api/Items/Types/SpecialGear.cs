using System;
using System.Linq;
using System.Text;
using FullPotential.Api.Data;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class SpecialGear : CombatItemBase, IResourceConsumer, IHasItemVisuals
    {
        private IResource _resourceType;
        private IItemVisuals _visuals;

        //Variables so they are serialized
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable NotAccessedField.Global
        public string CustomVisualsTypeId;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore NotAccessedField.Global
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

        public string VisualsTypeId => CustomVisualsTypeId;

        public IItemVisuals Visuals
        {
            get => _visuals;
            set
            {
                _visuals = value;
                CustomVisualsTypeId = _visuals?.TypeId.ToString();
            }
        }

        public SerializableKeyValuePair<string, string>[] CustomData = Array.Empty<SerializableKeyValuePair<string, string>>();

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var specialType = (ISpecialGear)RegistryType;
            var descriptionOverride = specialType.OverrideItemDescription(this, localizer, levelOfDetail);

            if (!descriptionOverride.IsNullOrWhiteSpace())
            {
                return descriptionOverride;
            }

            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, RegistryType.GetType().Name)}" + "\n");
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

        public void SetCustomData(string key, string value)
        {
            int i;
            for (i = 0; i < CustomData.Length; i++)
            {
                if (CustomData[i].Key != key)
                {
                    continue;
                }

                CustomData[i].Value = value;
                return;
            }

            Array.Resize(ref CustomData, CustomData.Length + 1);

            CustomData[CustomData.Length - 1] = new SerializableKeyValuePair<string, string>(key, value);
        }

        public string GetCustomData(string key)
        {
            return CustomData.FirstOrDefault(kvp => kvp.Key == key).Value;
        }
    }
}
