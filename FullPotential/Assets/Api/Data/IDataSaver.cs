using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Models.GameManagement;
using FullPotential.Models.Player;

namespace FullPotential.Api.Data
{
    public interface IDataSaver
    {
        UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails);

        UniTask SaveCharacterDataAsync(CharacterData playerData);

        UniTask SaveInventoryDataAsync(InventoryData inventoryData);

        UniTask<List<ItemData>> SaveInventoryAdditionsAndDeletionsAsync(string characterId, List<ItemData> newItems);
    }
}
