using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.GameManagement;
using FullPotential.Models.Player;
using FullPotential.Models.Utilities;

using Newtonsoft.Json.Linq;

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

                var response = request.downloadHandler.text.FromJson<GenericResponse>();

                if (!response.IsSuccess)
                {
                    return null;
                }

                var connectionDetails = ((JObject)response.Result).ToObject<ConnectionDetails>();

                return connectionDetails;
            }
        }

        public async UniTask<CharacterData> GetCharacterDataAsync(string characterId)
        {
            using (var request = UnityWebRequest.Get(BaseAddress + $"Character/GetCharacterData?characterId={Uri.EscapeDataString(characterId)}"))
            {
                SetAuthenticationHeader(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    return null;
                }

                var response = request.downloadHandler.text.FromJson<GenericResponse>();

                if (!response.IsSuccess)
                {
                    return null;
                }

                var characterData = ((JObject)response.Result).ToObject<CharacterData>();

                return characterData;
            }
        }

        public async UniTask<InventoryData> GetInventoryDataAsync(string characterId, bool reduced)
        {
            using (var request = UnityWebRequest.Get(BaseAddress + $"Character/GetInventoryData?characterId={Uri.EscapeDataString(characterId)}&minimal={reduced}"))
            {
                SetAuthenticationHeader(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    return null;
                }

                var response = request.downloadHandler.text.FromJson<GenericResponse>();

                if (!response.IsSuccess)
                {
                    return null;
                }

                var inventoryData = ((JObject)response.Result).ToObject<InventoryData>();

                return inventoryData;
            }
        }
    }
}
