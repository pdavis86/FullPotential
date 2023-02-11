using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IInventory : IDefensible
    {
        T GetItemWithId<T>(string id, bool logIfNotFound = true) where T : ItemBase;

        ItemBase GetItemInSlot(SlotGameObjectName slotGameObjectName);
    }
}
