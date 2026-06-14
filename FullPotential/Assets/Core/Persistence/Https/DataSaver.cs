using System.Net.Mime;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.GameManagement;
using FullPotential.Models.Player;

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

        public async UniTask SaveCharacterDataAsync(CharacterData playerData)
        {
            var json = playerData.ToJson();
            using (var request = UnityWebRequest.Post(BaseAddress + "Character/SaveCharacterData", json, MediaTypeNames.Application.Json))
            {
                SetAuthenticationHeader(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                }
            }
        }

        public async UniTask SaveInventoryDataAsync(InventoryData inventoryData)
        {
            var json = inventoryData.ToJson();
            using (var request = UnityWebRequest.Post(BaseAddress + "Character/SaveInventoryData", json, MediaTypeNames.Application.Json))
            {
                SetAuthenticationHeader(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                }
            }
        }
    }
}
