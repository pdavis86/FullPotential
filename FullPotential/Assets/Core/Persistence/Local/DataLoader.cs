using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Obsolete;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public class DataLoader : IDataLoader
    {
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

        public async UniTask<PlayerData> GetPlayerDataAsync(string username)
        {
            var filePath = Paths.GetPlayerSavePath(username);

            if (!System.IO.File.Exists(filePath))
            {
                return new PlayerData
                {
                    Username = username,
                    Settings = new CharacterSettings(),
                    Resources = new Dictionary<string, int>()
                };
            }

            var loadJson = System.IO.File.ReadAllText(filePath);
            var playerData = JsonUtility.FromJson<PlayerData>(loadJson);

            // todo: zzz v0.6 - remove this fall-back
            if (playerData.Resources == null)
            {
                var playerDataOld = JsonUtility.FromJson<PlayerDataOld>(loadJson);
                playerData.Resources = new Dictionary<string, int>();
                foreach (var item in playerDataOld.Resources)
                {
                    playerData.Resources[item.Key] = item.Value;
                }
            }

            await Task.Yield();

            return playerData;
        }

        public async UniTask<InventoryData> GetInventoryDataAsync(string username, bool reduced)
        {
            var filePath = Paths.GetInventorySavePath(username);
            InventoryData inventoryData;

            if (System.IO.File.Exists(filePath))
            {
                var loadJson = System.IO.File.ReadAllText(filePath);
                inventoryData = JsonUtility.FromJson<InventoryData>(loadJson);
            }
            else
            {
                // todo: zzz v0.6 - remove this fall-back
                filePath = Paths.GetPlayerSavePath(username);
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
    }
}
