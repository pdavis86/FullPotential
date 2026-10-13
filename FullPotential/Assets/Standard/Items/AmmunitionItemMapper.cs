using System;
using System.Collections.Generic;

using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Items
{
    public class AmmunitionItemMapper : IItemTypeMapping
    {
        public Type GroupType => typeof(IAmmunitionType);

        public ItemBase CreateNew()
        {
            return new Ammunition();
        }

        public ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties)
        {
            throw new NotImplementedException();
        }
    }
}
