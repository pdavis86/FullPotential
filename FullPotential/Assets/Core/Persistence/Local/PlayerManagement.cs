using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Obsolete;
using FullPotential.Core.Player;

using Unity.Netcode;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public class PlayerManagement : IPlayerManagement
    {
        private readonly bool _isDebugBuild = Debug.isDebugBuild;
        private readonly string _persistentDataPath = Application.persistentDataPath;
        private readonly List<string> _asapSaveUsernames = new List<string>();

        private bool _isSaving;

        public async Awaitable<PlayerData> LoadPlayerDataAsync(string username, bool reduced)
        {
            await Task.Yield();

            var filePath = GetPlayerSavePath(username);

            if (!System.IO.File.Exists(filePath))
            {
                return new PlayerData
                {
                    Username = username,
                    Settings = new CharacterSettings(),
                    Resources = Array.Empty<SerializableKeyValuePair<string, int>>(),
                    Inventory = new InventoryData()
                };
            }

            var loadJson = System.IO.File.ReadAllText(filePath);
            var playerData = JsonUtility.FromJson<PlayerData>(loadJson);

            if (reduced)
            {
                StripExtraData(playerData);
            }

            return playerData;
        }

        public async Awaitable SavePlayerDataAsapAsync(string username)
        {
            await Task.Yield();

            if (!_asapSaveUsernames.Contains(username))
            {
                _asapSaveUsernames.Add(username);
            }
        }

        public async Awaitable SavePlayerDataImmediatelyAsync(PlayerData playerData)
        {
            await Task.Yield();

            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError($"Tried to save player data for '{playerData.Username}' when not on the server");
                return;
            }

            if (!playerData.InventoryLoadedSuccessfully)
            {
                Debug.LogWarning($"Not saving player data for '{playerData.Username}' because the load failed");
                return;
            }

            Debug.Log($"Saving player data for {playerData.Username}");
            Save(playerData);
        }

        public async Awaitable SavePlayerDataBatchAsync(Dictionary<ulong, string> clientIdToUsernameMapping, bool allData)
        {
            await Task.Yield();

            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Tried saving when not on the server");
                return;
            }

            if (_isSaving)
            {
                Debug.LogWarning("Already saving");
                return;
            }

            //Debug.Log("Checking if anything to save. allData: " + allData);

            var playerDataCollection = new List<PlayerData>();
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                if (!clientIdToUsernameMapping.ContainsKey(kvp.Key))
                {
                    Debug.LogWarning($"Could not find username for client {kvp.Key}");
                    continue;
                }

                if (allData || _asapSaveUsernames.Contains(clientIdToUsernameMapping[kvp.Key]))
                {
                    playerDataCollection.Add(kvp.Value.PlayerObject.GetComponent<PlayerFighter>().GetPlayerSaveData());
                }
            }

            if (!playerDataCollection.Any())
            {
                return;
            }

            _isSaving = true;

            try
            {
                var tasks = playerDataCollection.Select(x => Task.Run(() => SavePlayerDataImmediately(x)));
                await Task.WhenAll(tasks);
            }
            finally
            {
                _isSaving = false;
            }
        }

        private void SavePlayerDataImmediately(PlayerData playerData)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError($"Tried to save player data for '{playerData.Username}' when not on the server");
            }

            if (!playerData.InventoryLoadedSuccessfully)
            {
                Debug.LogWarning($"Not saving player data for '{playerData.Username}' because the load failed");
                return;
            }

            Debug.Log($"Saving player data for {playerData.Username}");

            Save(playerData);

            _asapSaveUsernames.Remove(playerData.Username);
        }

        private void Save(PlayerData playerData)
        {
            var prettyPrint = _isDebugBuild;
            var saveJson = JsonUtility.ToJson(playerData, prettyPrint);
            System.IO.File.WriteAllText(GetPlayerSavePath(playerData.Username), saveJson);

            _asapSaveUsernames.Remove(playerData.Username);
        }

        private string GetPlayerSavePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("No username supplied");
            }

            return _persistentDataPath + "/" + username + ".json";
        }

        private void StripExtraData(PlayerData playerData)
        {
            var equippedItemIds = playerData.Inventory.EquippedItems.Select(x => x.Value);

            playerData.Inventory.Accessories = playerData.Inventory.Accessories.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            playerData.Inventory.Armor = playerData.Inventory.Armor.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            playerData.Inventory.Loot = playerData.Inventory.Loot.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            playerData.Inventory.Consumers = playerData.Inventory.Consumers.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            playerData.Inventory.Weapons = playerData.Inventory.Weapons.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
        }
    }
}
