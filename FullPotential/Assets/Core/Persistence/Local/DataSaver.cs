using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
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
            // Override the save file name
            var username = GameManager.Instance.LocalGameDataStore.SignInResult?.Username;
            var filePath = Paths.GetCharacterSavePath(username);

            var saveJson = playerData.ToJson();
            System.IO.File.WriteAllText(filePath, saveJson);

            await Task.Yield();
        }

        public async UniTask SaveInventoryDataAsync(InventoryData inventoryData)
        {
            // Override the save file name
            var username = GameManager.Instance.LocalGameDataStore.SignInResult?.Username;
            var filePath = Paths.GetInventorySavePath(username);

            var saveJson = inventoryData.ToJson();
            System.IO.File.WriteAllText(filePath, saveJson);

            await Task.Yield();
        }
    }
}
