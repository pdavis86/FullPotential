using System;
using System.Collections.Generic;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Inventory;
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
    public class CustomGearItemMapper : IItemTypeMapping
    {
        private readonly ITypeRegistry _typeRegistry;

        public Type GroupType => typeof(ICustomGearType);

        public CustomGearItemMapper(
            ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public ItemBase CreateNew()
        {
            return DependenciesContext.Dependencies.CreateInstance<CustomGear>();
        }

        public ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties)
        {
            // todo: zzz v0.6 - Cannot inject IResultFactory as it is not ready to be used yet
            var resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();

            var valuePoolTypeId = properties.ContainsKey(IValuePoolConsumer.PropertyKeyResourceTypeId)
                   ? properties[IValuePoolConsumer.PropertyKeyResourceTypeId]
                   : properties.GetStringValueOrNull(IValuePoolConsumer.PropertyKeyValuePoolTypeId);

            var valuePoolType = valuePoolTypeId.IsNullOrWhiteSpace()
                ? null
                : _typeRegistry.GetRegisteredByTypeId<IValuePoolType>(valuePoolTypeId);

            var customGear = DependenciesContext.Dependencies.CreateInstance<CustomGear>();
            customGear.RegistryType = _typeRegistry.GetRegisteredByTypeId<ICustomGearType>(itemTypeId);
            customGear.ValuePoolType = valuePoolType;
            customGear.Attributes = new Dictionary<AttributeTypeId, int>
            {
                { Strength.Id, resultFactory.ComputeAttribute(components, Strength.Id)},
                { Efficiency.Id, resultFactory.ComputeAttribute(components, Efficiency.Id) },
                { Range.Id, resultFactory.ComputeAttribute(components, Range.Id) },
                { Accuracy.Id, resultFactory.ComputeAttribute(components, Accuracy.Id) },
                { Speed.Id, resultFactory.ComputeAttribute(components, Speed.Id) },
                { Recovery.Id, resultFactory.ComputeAttribute(components, Recovery.Id) },
                { Duration.Id, resultFactory.ComputeAttribute(components, Duration.Id) },
                { Luck.Id, resultFactory.ComputeAttribute(components, Luck.Id) },
            };
            customGear.Properties = properties;

            resultFactory.SetEffects(customGear, components);
            resultFactory.SetItemName(customGear, true);

            return customGear;
        }
    }
}
