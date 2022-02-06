using System.Collections.Generic;
using FullPotential.Api.Combat;
using FullPotential.Api.Enums;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerInventory : IDefensible
    {
        T GetItemWithId<T>(string id, bool logIfNotFound = true) where T : ItemBase;

        ItemBase GetItemInSlot(SlotGameObjectName slotGameObjectName);

        List<ItemBase> GetComponentsFromIds(string[] componentIds);

        List<string> ValidateIsCraftable(string[] componentIds, ItemBase itemToCraft);

        IEnumerable<ItemBase> GetCompatibleItemsForSlot(IGear.GearCategory? gearCategory);

        KeyValuePair<SlotGameObjectName, Data.EquippedItem>? GetEquippedWithItemId(string itemId);
    }
}
