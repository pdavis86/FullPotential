using System;
using System.Collections.Generic;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Standard.Items
{
    public class AccessoryItemMapper : IItemTypeMapping
    {
        private readonly ITypeRegistry _typeRegistry;

        public Type GroupType => typeof(IAccessoryType);

        public AccessoryItemMapper(
            ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public ItemBase CreateNew()
        {
            return DependenciesContext.Dependencies.CreateInstance<Accessory>();
        }

        public ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties)
        {
            // todo: zzz v0.6 - Cannot inject IResultFactory as it is not ready to be used yet
            var resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();

            var accessory = DependenciesContext.Dependencies.CreateInstance<Accessory>();

            accessory.RegistryType = _typeRegistry.GetRegisteredByTypeId<IAccessoryType>(itemTypeId);
            accessory.Attributes = new Dictionary<string, string>
            {
                { Strength.TypeIdString, resultFactory.ComputeAttribute(components, Strength.TypeIdString).ToString() },
                { Efficiency.TypeIdString, resultFactory.ComputeAttribute(components, Efficiency.TypeIdString).ToString() },
                { Speed.TypeIdString, resultFactory.ComputeAttribute(components, Speed.TypeIdString).ToString() },
                { Recovery.TypeIdString, resultFactory.ComputeAttribute(components, Recovery.TypeIdString).ToString() },
            };
            accessory.Properties = properties;

            resultFactory.SetEffects(accessory, components);

            if (!itemName.IsNullOrWhiteSpace())
            {
                accessory.Name = itemName;
            }
            else
            {
                resultFactory.SetItemName(accessory, false);
            }

            return accessory;
        }
    }
}
