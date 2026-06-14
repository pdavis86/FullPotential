using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.GameManagement;
using FullPotential.Models.Player;

namespace FullPotential.Core.Persistence.Local
{
    public class DataSaver : IDataSaver
    {
        public async UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails)
        {
            // Do nothing
            await Task.Yield();
        }

        public async UniTask SaveCharacterDataAsync(CharacterData playerData)
        {
            var saveJson = playerData.ToJson();
            var filePath = Paths.GetCharacterSavePath(playerData.CharacterId);
            System.IO.File.WriteAllText(filePath, saveJson);

            await Task.Yield();
        }

        public async UniTask SaveInventoryDataAsync(InventoryData inventoryData)
        {
            var saveJson = inventoryData.ToJson();
            var filePath = Paths.GetInventorySavePath(inventoryData.CharacterId);
            System.IO.File.WriteAllText(filePath, saveJson);

            await Task.Yield();
        }
    }
}
