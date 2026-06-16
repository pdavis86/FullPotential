using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Models.GameManagement;
using FullPotential.Models.Player;

namespace FullPotential.Api.Data
{
    public interface IDataLoader
    {
        UniTask<ConnectionDetails> GetConnectionDetailsAsync();

        UniTask<CharacterData> GetCharacterDataAsync(string characterId);

        UniTask<InventoryData> GetInventoryDataAsync(string characterId, bool reduced);

        UniTask<List<ItemData>> GetInventoryItemDataAsync(string characterId, IEnumerable<string> itemIds);
    }
}
