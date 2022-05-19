using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry.Base;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IInventory : IDefensible
    {
        T GetItemWithId<T>(string id, bool logIfNotFound = true) where T : ItemBase;

        ItemBase GetItemInSlot(SlotGameObjectName slotGameObjectName);

        List<ItemBase> GetComponentsFromIds(string[] componentIds);
    }
}
