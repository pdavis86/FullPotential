using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Logging;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.GameManagement;
using FullPotential.Models.Player;
using FullPotential.Models.Utilities;

using Newtonsoft.Json.Linq;

using UnityEngine.Networking;

namespace FullPotential.Core.Persistence.Https
{
    public class DataLoader : HttpsPersistenceBase, IDataLoader
    {
        private readonly IAuditor _logger;

        public DataLoader(IAuditorFactory auditorFactory, ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
            _logger = auditorFactory.Create(this);
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

        public UniTask<List<ItemData>> GetInventoryItemDataAsync(string characterId, IEnumerable<string> itemIds)
        {
            // todo: GetInventoryItemDataAsync()
            throw new NotImplementedException();
        }

        protected override IAuditor GetLogger()
        {
            return _logger;
        }
    }
}
