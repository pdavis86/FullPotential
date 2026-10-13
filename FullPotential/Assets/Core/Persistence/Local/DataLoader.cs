using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.GameManagement;
using FullPotential.Models.LivingEntity;

namespace FullPotential.Core.Persistence.Local
{
    public class DataLoader : IDataLoader
    {
        public UniTask<ConnectionDetails> GetConnectionDetailsAsync()
        {
            return UniTask.FromResult(new ConnectionDetails
            {
                Address = "127.0.0.1",
                Port = 7180,
                Status = InstanceState.Available
            });
        }

        public UniTask<CharacterData> GetCharacterDataAsync(string characterId)
        {
            var filePath = Paths.GetCharacterSavePath(characterId);

            if (!System.IO.File.Exists(filePath))
            {
                return UniTask.FromResult(new CharacterData
                {
                    CharacterId = characterId,
                    Settings = new Dictionary<string, string>(),
                    ValuePools = new Dictionary<string, int>()
                });
            }

            var loadJson = System.IO.File.ReadAllText(filePath);
            var characterData = loadJson.FromJson<CharacterData>();

            return UniTask.FromResult(characterData);
        }

        public UniTask<InventoryData> GetInventoryDataAsync(string characterId, bool reduced)
        {
            var filePath = Paths.GetInventorySavePath(characterId);

            if (!System.IO.File.Exists(filePath))
            {
                return UniTask.FromResult(new InventoryData());
            }

            var loadJson = System.IO.File.ReadAllText(filePath);
            var inventoryData = loadJson.FromJson<InventoryData>();

            if (reduced)
            {
                var equippedItemIds = inventoryData.EquippedItems.Select(x => x.Value);
                inventoryData.Items = inventoryData.Items.Where(x => equippedItemIds.Contains(x.Id)).ToList();
            }

            return UniTask.FromResult(inventoryData);
        }

        public async UniTask<List<ItemData>> GetInventoryItemDataAsync(string characterId, IEnumerable<string> itemIds)
        {
            var everything = await GetInventoryDataAsync(characterId, false);

            return everything.Items.Where(x => itemIds.Contains(x.Id)).ToList();
        }
    }
}
