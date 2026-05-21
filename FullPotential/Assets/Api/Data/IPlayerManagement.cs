using Cysharp.Threading.Tasks;

using FullPotential.Api.Data.Models;

namespace FullPotential.Api.Data
{
    public interface IPlayerManagement
    {
        UniTask<PlayerData> GetPlayerDataAsync(string username);

        UniTask SavePlayerDataAsync(PlayerData playerData);

        UniTask<InventoryData> GetInventoryDataAsync(string username, bool reduced);

        UniTask SaveInventoryChangesAsync(InventoryChanges inventoryChanges);
    }
}
