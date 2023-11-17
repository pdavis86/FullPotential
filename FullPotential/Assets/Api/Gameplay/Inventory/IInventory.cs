using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IInventory : IDefensible
    {
        T GetItemWithId<T>(string id, bool logIfNotFound = true) where T : ItemBase;

        ItemBase GetItemInSlot(string slotId);
        
        ItemStack TakeItemStack(string typeId, int maxSize);

        int GetItemStackTotal(string typeId);
        
        bool HasTypeEquipped(string slotId);
    }
}
