using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Data.Models;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class InventoryDataExtensions
    {
        public static IEnumerable<ItemBase> GetEquipableItems(this InventoryData inventoryData)
        {
            return Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(inventoryData.Accessories)
                .UnionIfNotNull(inventoryData.Armor)
                .UnionIfNotNull(inventoryData.Consumers)
                .UnionIfNotNull(inventoryData.SpecialGear)
                .UnionIfNotNull(inventoryData.Weapons);
        }

        public static IEnumerable<ItemBase> GetNonItemStacks(this InventoryData inventoryData)
        {
            return Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(inventoryData.Accessories)
                .UnionIfNotNull(inventoryData.Armor)
                .UnionIfNotNull(inventoryData.Consumers)
                .UnionIfNotNull(inventoryData.Loot)
                .UnionIfNotNull(inventoryData.SpecialGear)
                .UnionIfNotNull(inventoryData.Weapons);
        }

        public static IEnumerable<ItemBase> GetAllItems(this InventoryData inventoryData)
        {
            return Enumerable.Empty<ItemBase>()
                 .UnionIfNotNull(inventoryData.Accessories)
                 .UnionIfNotNull(inventoryData.Armor)
                 .UnionIfNotNull(inventoryData.Consumers)
                 .UnionIfNotNull(inventoryData.ItemStacks)
                 .UnionIfNotNull(inventoryData.Loot)
                 .UnionIfNotNull(inventoryData.SpecialGear)
                 .UnionIfNotNull(inventoryData.Weapons);
        }
    }
}
