using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;

using UnityEngine;
using UnityEngine.Networking;

namespace FullPotential.Core.Persistence.Https
{
    public class DataLoader : HttpsPersistenceBase, IDataLoader
    {
        public DataLoader(ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
        }

        public async UniTask<ConnectionDetails> GetConnectionDetailsAsync()
        {
            using (var request = UnityWebRequest.Get(BaseAddress + "Instance/GetConnectionDetails"))
            {
                SetAuthenticationHeader(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    return null;
                }

                var result = JsonUtility.FromJson<ConnectionDetails>(request.downloadHandler.text);
                return result;
            }
        }

        public UniTask<PlayerData> GetPlayerDataAsync(string username)
        {
            // todo: GetPlayerDataAsync
            throw new NotImplementedException();
        }

        public UniTask SavePlayerDataAsync(PlayerData playerData)
        {
            // todo: SavePlayerDataAsync
            throw new NotImplementedException();
        }

        public UniTask<InventoryData> GetInventoryDataAsync(string username, bool reduced)
        {
            // todo: GetInventoryDataAsync
            throw new NotImplementedException();
        }

        public UniTask SaveInventoryChangesAsync(InventoryChanges inventoryChanges)
        {
            // todo: SaveInventoryChangesAsync
            throw new NotImplementedException();
        }
    }
}
