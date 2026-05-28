using Cysharp.Threading.Tasks;

using FullPotential.Api.Data.Models;

namespace FullPotential.Api.Data
{
    public interface IDataSaver
    {
        UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails);

        UniTask SavePlayerDataAsync(PlayerData playerData);

        UniTask SaveInventoryChangesAsync(InventoryChanges inventoryChanges);
    }
}
