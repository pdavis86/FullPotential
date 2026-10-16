using System;
using System.Linq;
using System.Text;

using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Utilities.Extensions;

using UnityEngine;

namespace FullPotential.Standard.Items
{
    public class SpecialGear : CombatItemBase, IResourceConsumer, IHasItemVisuals
    {
        private readonly ITypeRegistry _typeRegistry;

        public IResourceType ResourceType { get; set; }

        public IItemVisuals Visuals { get; set; }

        public SerializableKeyValuePair<string, string>[] CustomData = Array.Empty<SerializableKeyValuePair<string, string>>();

        public SpecialGear(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full)
        {
            var specialType = (ISpecialGearType)RegistryType;
            var descriptionOverride = specialType.OverrideItemDescription(this, localizer, levelOfDetail);

            if (!descriptionOverride.IsNullOrWhiteSpace())
            {
                return descriptionOverride;
            }

            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {Name}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(RegistryType)}" + "\n");
            }

            if (ResourceType != null)
            {
                var resourceConsumptionType = localizer.Translate(ResourceType);

                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.GetIntValue(Efficiency.TypeIdString),
                    nameof(Efficiency),
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

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            return null;
        }

        public void InstantiateItemVisuals(IPlayerFighter playerFighter, string slotId)
        {
            if (Visuals == null)
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                Visuals.PrefabAddress,
                prefab =>
                {
                    var newObj = UnityEngine.Object.Instantiate(prefab, playerFighter.GraphicsTransform);
                    playerFighter.Inventory.SetEquippedItemVisuals(slotId, newObj);
                });
        }
    }
}
