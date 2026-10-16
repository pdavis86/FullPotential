using System;
using System.Collections.Generic;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Items
{
    public class ArmorItemMapper : IItemTypeMapping
    {
        private readonly ITypeRegistry _typeRegistry;

        public Type GroupType => typeof(IArmorType);

        public ArmorItemMapper(
            ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public ItemBase CreateNew()
        {
            return DependenciesContext.Dependencies.CreateInstance<Armor>();
        }

        public ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties)
        {
            // todo: zzz v0.6 - Cannot inject IResultFactory as it is not ready to be used yet
            var resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();

            var armor = DependenciesContext.Dependencies.CreateInstance<Armor>();

            armor.RegistryType = _typeRegistry.GetRegisteredByTypeId<IArmorType>(itemTypeId);
            armor.Attributes = new Dictionary<string, string>
            {
                { Strength.TypeIdString, resultFactory.ComputeAttribute(components, Strength.TypeIdString).ToString() },
            };
            armor.Properties = properties;

            resultFactory.SetEffects(armor, components);
            resultFactory.SetItemName(armor, false);

            return armor;
        }
    }
}
