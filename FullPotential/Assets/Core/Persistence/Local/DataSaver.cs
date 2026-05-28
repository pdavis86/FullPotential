using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public class DataSaver : IDataSaver
    {
        public async UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails)
        {
            // Do nothing
            await Task.Yield();
        }

        public async UniTask SavePlayerDataAsync(PlayerData playerData)
        {
            var saveJson = JsonUtility.ToJson(playerData, true);
            System.IO.File.WriteAllText(Paths.GetPlayerSavePath(playerData.Username), saveJson);

            await Task.Yield();
        }

        public async UniTask SaveInventoryChangesAsync(InventoryChanges inventoryChanges)
        {
            var inventoryData = (InventoryData)inventoryChanges;
            var saveJson = JsonUtility.ToJson(inventoryData, true);
            System.IO.File.WriteAllText(Paths.GetInventorySavePath(inventoryData.Username), saveJson);

            await Task.Yield();
        }
    }
}
