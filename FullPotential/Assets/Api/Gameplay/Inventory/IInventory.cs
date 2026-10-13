using System.Collections.Generic;

using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IInventory
    {
        List<ItemBase> GetComponentsFromIds(string[] componentIds);

        IEnumerable<string> ValidateIsCraftable(string[] componentIds, ItemBase itemToCraft);
    }
}
