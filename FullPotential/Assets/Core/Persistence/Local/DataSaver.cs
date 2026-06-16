using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Items;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.GameManagement;
using FullPotential.Models.Player;

namespace FullPotential.Core.Persistence.Local
{
    public class DataSaver : IDataSaver
    {
        private IDataLoader _dataLoader;

        public DataSaver(IDataLoader dataLoader)
        {
            _dataLoader = dataLoader;
        }

        public async UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails)
        {
            // Do nothing
            await Task.Yield();
        }

        public async UniTask SaveCharacterDataAsync(CharacterData playerData)
        {
            var filePath = Paths.GetCharacterSavePath(playerData.CharacterId);

            var saveJson = playerData.ToJson();
            System.IO.File.WriteAllText(filePath, saveJson);

            await Task.Yield();
        }

        public async UniTask SaveInventoryDataAsync(InventoryData inventoryData)
        {
            var currentData = await _dataLoader.GetInventoryDataAsync(inventoryData.CharacterId, false);

            foreach (var existingItem in currentData.Items)
            {
                var match = inventoryData.Items.FirstOrDefault(x => x.Id == existingItem.Id && !x.IsDeleted);

                if (match != null)
                {
                    continue;
                }

                inventoryData.Items.Add(existingItem);
            }

            inventoryData.Items.RemoveAll(x => x.IsDeleted);

            if (inventoryData.EquippedItems == null)
            {
                inventoryData.EquippedItems = currentData.EquippedItems;
            }

            var filePath = Paths.GetInventorySavePath(inventoryData.CharacterId);
            var saveJson = inventoryData.ToJson();
            System.IO.File.WriteAllText(filePath, saveJson);
        }

        public async UniTask<List<ItemData>> SaveInventoryAdditionsAndDeletionsAsync(string characterId, List<ItemData> newItems)
        {
            var currentData = await _dataLoader.GetInventoryDataAsync(characterId, false);

            foreach (var newItem in newItems)
            {
                newItem.Id = Guid.NewGuid().ToString();
                currentData.Items.Add(newItem);
            }

            var filePath = Paths.GetInventorySavePath(characterId);
            var saveJson = currentData.ToJson();
            System.IO.File.WriteAllText(filePath, saveJson);

            await Task.Yield();

            return newItems;
        }
    }
}
