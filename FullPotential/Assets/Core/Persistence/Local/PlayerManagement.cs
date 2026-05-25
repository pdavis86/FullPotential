using System;
using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;
using FullPotential.Api.Obsolete;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public class PlayerManagement : IPlayerManagement
    {
        private readonly string _persistentDataPath = Application.persistentDataPath;

        public async UniTask<PlayerData> GetPlayerDataAsync(string username)
        {
            var filePath = GetPlayerSavePath(username);

            if (!System.IO.File.Exists(filePath))
            {
                return new PlayerData
                {
                    Username = username,
                    Settings = new CharacterSettings(),
                    Resources = Array.Empty<SerializableKeyValuePair<string, int>>(),
                };
            }

            var loadJson = System.IO.File.ReadAllText(filePath);
            var playerData = JsonUtility.FromJson<PlayerData>(loadJson);

            await Task.Yield();

            return playerData;
        }

        public async UniTask SavePlayerDataAsync(PlayerData playerData)
        {
            var saveJson = JsonUtility.ToJson(playerData, true);
            System.IO.File.WriteAllText(GetPlayerSavePath(playerData.Username), saveJson);

            await Task.Yield();
        }

        public async UniTask<InventoryData> GetInventoryDataAsync(string username, bool reduced)
        {
            var filePath = GetInventorySavePath(username);
            InventoryData inventoryData;

            if (System.IO.File.Exists(filePath))
            {
                var loadJson = System.IO.File.ReadAllText(filePath);
                inventoryData = JsonUtility.FromJson<InventoryData>(loadJson);
            }
            else
            {
                // todo: zzz v0.6 - remove this fall-back
                filePath = GetPlayerSavePath(username);
                if (!System.IO.File.Exists(filePath))
                {
                    return new InventoryData();
                }

                var loadJsonOld = System.IO.File.ReadAllText(filePath);
                var playerDataOld = JsonUtility.FromJson<PlayerDataOld>(loadJsonOld);
                playerDataOld.Inventory.Username = username;
                inventoryData = playerDataOld.Inventory;
            }

            if (reduced)
            {
                inventoryData.Loot = null;
                inventoryData.ShapeMapping = null;
                inventoryData.ItemStacks = null;

                var equippedItemIds = inventoryData.EquippedItems.Select(x => x.Value);
                inventoryData.Accessories = inventoryData.Accessories.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
                inventoryData.Armor = inventoryData.Armor.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
                inventoryData.Weapons = inventoryData.Weapons.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
                inventoryData.Consumers = inventoryData.Consumers.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
                inventoryData.SpecialGear = inventoryData.SpecialGear.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            }

            await Task.Yield();

            return inventoryData;
        }

        public async UniTask SaveInventoryChangesAsync(InventoryChanges inventoryChanges)
        {
            var inventoryData = (InventoryData)inventoryChanges;
            var saveJson = JsonUtility.ToJson(inventoryData, true);
            System.IO.File.WriteAllText(GetInventorySavePath(inventoryData.Username), saveJson);

            await Task.Yield();
        }

        private string GetPlayerSavePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("No username supplied");
            }

            return _persistentDataPath + "/" + username + ".json";
        }

        private string GetInventorySavePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("No username supplied");
            }

            return _persistentDataPath + "/" + username + "_inventory.json";
        }
    }
}
