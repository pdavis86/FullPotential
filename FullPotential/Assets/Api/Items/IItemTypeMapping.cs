using System;
using System.Collections.Generic;

using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Items
{
    public interface IItemTypeMapping
    {
        Type GroupType { get; }

        ItemBase CreateNew();

        ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties);
    }
}
