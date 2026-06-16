using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.GameManagement;
using FullPotential.Models.Player;

using Newtonsoft.Json;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public class DataLoader : IDataLoader
    {
        private IItemFactory _itemFactory;

        public DataLoader(IItemFactory itemFactory)
        {
            _itemFactory = itemFactory;
        }

        public async UniTask<ConnectionDetails> GetConnectionDetailsAsync()
        {
            await Task.Yield();

            return new ConnectionDetails
            {
                Address = "127.0.0.1",
                Port = 7180,
                Status = InstanceState.Available
            };
        }

        public async UniTask<CharacterData> GetCharacterDataAsync(string characterId)
        {
            var filePath = Paths.GetCharacterSavePath(characterId);

            if (!System.IO.File.Exists(filePath))
            {
                return new CharacterData
                {
                    CharacterId = characterId,
                    Settings = new Dictionary<string, string>(),
                    ValuePools = new Dictionary<string, int>()
                };
            }

            var loadJson = System.IO.File.ReadAllText(filePath);
            var characterData = loadJson.FromJson<CharacterData>();

            // todo: zzz v0.6 - remove this fall-back
            if (characterData.ValuePools == null)
            {
                var playerDataOld = JsonUtility.FromJson<PlayerDataOld>(loadJson);
                characterData.ValuePools = new Dictionary<string, int>();
                foreach (var item in playerDataOld.Resources)
                {
                    characterData.ValuePools[item.Key] = item.Value;
                }
            }

            await Task.Yield();

            return characterData;
        }

        public async UniTask<InventoryData> GetInventoryDataAsync(string characterId, bool reduced)
        {
            var filePath = Paths.GetInventorySavePath(characterId);

            InventoryData inventoryData;

            if (System.IO.File.Exists(filePath))
            {
                var loadJson = System.IO.File.ReadAllText(filePath);
                try
                {
                    inventoryData = loadJson.FromJson<InventoryData>();
                }
                catch (JsonSerializationException)
                {
                    // todo: zzz v0.6 - remove this fall-back
                    var inventoryDataOld = JsonUtility.FromJson<InventoryDataOld>(loadJson);
                    inventoryData = GetInventoryDataV2FromV1(characterId, inventoryDataOld);
                }
            }
            else
            {
                // todo: zzz v0.6 - remove this fall-back
                filePath = Paths.GetCharacterSavePath(characterId);
                if (!System.IO.File.Exists(filePath))
                {
                    return new InventoryData();
                }

                var loadJsonOld = System.IO.File.ReadAllText(filePath);
                var playerDataOld = JsonUtility.FromJson<PlayerDataOld>(loadJsonOld);
                inventoryData = GetInventoryDataV2FromV1(characterId, playerDataOld.Inventory);
            }

            if (reduced)
            {
                var equippedItemIds = inventoryData.EquippedItems.Select(x => x.Value);
                inventoryData.Items = inventoryData.Items.Where(x => equippedItemIds.Contains(x.Id)).ToList();
            }

            await Task.Yield();

            return inventoryData;
        }

        public async UniTask<List<ItemData>> GetInventoryItemDataAsync(string characterId, IEnumerable<string> itemIds)
        {
            var everything = await GetInventoryDataAsync(characterId, false);

            return everything.Items.Where(x => itemIds.Contains(x.Id)).ToList();
        }

        private InventoryData GetInventoryDataV2FromV1(string characterId, InventoryDataOld inventoryDataOld)
        {
            var allItems = Enumerable.Empty<ItemBase>()
                 .UnionIfNotNull(inventoryDataOld.Accessories)
                 .UnionIfNotNull(inventoryDataOld.Armor)
                 .UnionIfNotNull(inventoryDataOld.Consumers)
                 .UnionIfNotNull(inventoryDataOld.ItemStacks)
                 .UnionIfNotNull(inventoryDataOld.Loot)
                 .UnionIfNotNull(inventoryDataOld.SpecialGear)
                 .UnionIfNotNull(inventoryDataOld.Weapons);

            var inventoryData = new InventoryData();
            inventoryData.CharacterId = characterId;
            inventoryData.Items = allItems.Select(x => _itemFactory.GetDataFromItem(characterId, x)).ToList();
            inventoryData.EquippedItems = inventoryDataOld.EquippedItems.ToDictionary(x => x.Key, x => x.Value);
            inventoryData.IsDirty = true;
            return inventoryData;
        }
    }
}
