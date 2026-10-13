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
            // todo zzz v0.6 - Cannot inject IResultFactory as it is not ready to be used yet
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
                { Strength.TypeId, resultFactory.ComputeAttribute(components, Strength.TypeId).ToString() },
                { Efficiency.TypeId, resultFactory.ComputeAttribute(components, Efficiency.TypeId).ToString() },
                { Range.TypeId, resultFactory.ComputeAttribute(components, Range.TypeId).ToString() },
                { Accuracy.TypeId, resultFactory.ComputeAttribute(components, Accuracy.TypeId).ToString() },
                { Speed.TypeId, resultFactory.ComputeAttribute(components, Speed.TypeId).ToString() },
                { Recovery.TypeId, resultFactory.ComputeAttribute(components, Recovery.TypeId).ToString() },
                { Duration.TypeId, resultFactory.ComputeAttribute(components, Duration.TypeId).ToString() },
                { Luck.TypeId, resultFactory.ComputeAttribute(components, Luck.TypeId).ToString() },
            };
            specialGear.Properties = properties;

            resultFactory.SetEffects(specialGear, components);
            resultFactory.SetItemName(specialGear, true);

            return specialGear;
        }
    }
}
