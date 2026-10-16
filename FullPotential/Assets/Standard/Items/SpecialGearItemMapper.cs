using System;
using System.Collections.Generic;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Utilities.Extensions;

using Range = FullPotential.Api.Gameplay.Attributes.Range;

namespace FullPotential.Standard.Items
{
    public class SpecialGearItemMapper : IItemTypeMapping
    {
        private readonly ITypeRegistry _typeRegistry;

        public Type GroupType => typeof(ISpecialGearType);

        public SpecialGearItemMapper(
            ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public ItemBase CreateNew()
        {
            return DependenciesContext.Dependencies.CreateInstance<SpecialGear>();
        }

        public ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties)
        {
            // todo: zzz v0.6 - Cannot inject IResultFactory as it is not ready to be used yet
            var resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();

            var resourceTypeId = properties.GetStringValueOrNull(IResourceConsumer.PropertyKeyResourceTypeId);

            var resourceType = resourceTypeId.IsNullOrWhiteSpace()
                ? null
                : _typeRegistry.GetRegisteredByTypeId<IResourceType>(resourceTypeId);

            var specialGear = DependenciesContext.Dependencies.CreateInstance<SpecialGear>();
            specialGear.RegistryType = _typeRegistry.GetRegisteredByTypeId<ISpecialGearType>(itemTypeId);
            specialGear.ResourceType = resourceType;
            specialGear.Attributes = new Dictionary<string, string>
            {
                { Strength.TypeIdString, resultFactory.ComputeAttribute(components, Strength.TypeIdString).ToString() },
                { Efficiency.TypeIdString, resultFactory.ComputeAttribute(components, Efficiency.TypeIdString).ToString() },
                { Range.TypeIdString, resultFactory.ComputeAttribute(components, Range.TypeIdString).ToString() },
                { Accuracy.TypeIdString, resultFactory.ComputeAttribute(components, Accuracy.TypeIdString).ToString() },
                { Speed.TypeIdString, resultFactory.ComputeAttribute(components, Speed.TypeIdString).ToString() },
                { Recovery.TypeIdString, resultFactory.ComputeAttribute(components, Recovery.TypeIdString).ToString() },
                { Duration.TypeIdString, resultFactory.ComputeAttribute(components, Duration.TypeIdString).ToString() },
                { Luck.TypeIdString, resultFactory.ComputeAttribute(components, Luck.TypeIdString).ToString() },
            };
            specialGear.Properties = properties;

            resultFactory.SetEffects(specialGear, components);
            resultFactory.SetItemName(specialGear, true);

            return specialGear;
        }
    }
}
