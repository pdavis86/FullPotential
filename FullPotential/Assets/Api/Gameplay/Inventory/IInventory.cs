using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;

namespace FullPotential.Api.Gameplay.Inventory
{
    //todo: delete IInventory in favour of just InventoryBase? Why have both? 
    public interface IInventory : IDefensible
    {
        public const string EventIdSlotChange = "9c7972de-4136-4825-aaa3-11925ad049ee";

        T GetItemWithId<T>(string id, bool logIfNotFound = true) where T : ItemBase;

        ItemBase GetItemInSlot(string slotId);
        
        ItemStack TakeItemStack(string typeId, int maxSize);

        int GetItemStackTotal(string typeId);
    }
}
