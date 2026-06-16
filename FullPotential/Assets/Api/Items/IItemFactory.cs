using FullPotential.Api.Items.Base;
using FullPotential.Models.Player;

namespace FullPotential.Api.Items
{
    public interface IItemFactory
    {
        ItemBase GetItemFromData(ItemData model);

        ItemData GetDataFromItem(string characterId, ItemBase item);

        void FillTypesFromIds(ItemBase item);
    }
}
