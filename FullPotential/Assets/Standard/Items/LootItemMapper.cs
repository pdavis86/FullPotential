using System;
using System.Collections.Generic;

using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;

namespace FullPotential.Standard.Items
{
    public class LootItemMapper : IItemTypeMapping
    {
        public Type GroupType => typeof(ILootType);

        public ItemBase CreateNew()
        {
            return new Loot();
        }

        public ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties)
        {
            throw new NotImplementedException();
        }
    }
}
