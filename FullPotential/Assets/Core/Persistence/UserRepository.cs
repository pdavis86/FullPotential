using System;
using System.Linq;
using FullPotential.Api.Data;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Persistence;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly bool _isDebugBuild = Debug.isDebugBuild;
        private readonly string _persistentDataPath = Application.persistentDataPath;

        public string SignIn(string username, string password)
        {
            var token = string.IsNullOrWhiteSpace(username)
                ? SystemInfo.deviceUniqueIdentifier
                : username;

            return token;
        }

        public string GetUsernameFromToken(string token)
        {
            //todo: Username and token are never the same
            return token;
        }

        public PlayerData Load(string token, string username, bool reduced)
        {
            //todo: Username and token are never the same
            if (string.IsNullOrWhiteSpace(username))
            {
                username = token;
            }

            var filePath = GetPlayerSavePath(username);

            if (!System.IO.File.Exists(filePath))
            {
                return new PlayerData
                {
                    Username = username,
                    Settings = new PlayerSettings(),
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

        public void Save(PlayerData playerData)
        {
            var prettyPrint = _isDebugBuild;
            var saveJson = JsonUtility.ToJson(playerData, prettyPrint);
            System.IO.File.WriteAllText(GetPlayerSavePath(playerData.Username), saveJson);
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
