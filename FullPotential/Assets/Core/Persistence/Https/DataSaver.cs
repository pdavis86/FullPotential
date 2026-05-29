using System;
using System.Net.Mime;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models;

using UnityEngine;
using UnityEngine.Networking;

namespace FullPotential.Core.Persistence.Https
{
    public class DataSaver : HttpsPersistenceBase, IDataSaver
    {
        public DataSaver(ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
        }

        public async UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails)
        {
            var json = JsonUtility.ToJson(connectionDetails);
            using (var request = UnityWebRequest.Post(BaseAddress + "Instance/SaveConnectionDetails", json, MediaTypeNames.Application.Json))
            {
                SetAuthenticationHeader(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                }
            }
        }

        public async UniTask SavePlayerDataAsync(PlayerData playerData)
        {
            var json = playerData.ToJson();
            using (var request = UnityWebRequest.Post(BaseAddress + "Character/SavePlayerData", json, MediaTypeNames.Application.Json))
            {
                SetAuthenticationHeader(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                }
            }
        }

        public UniTask SaveInventoryChangesAsync(InventoryChanges inventoryChanges)
        {
            // todo: SaveInventoryChangesAsync
            return UniTask.CompletedTask;
        }
    }
}
