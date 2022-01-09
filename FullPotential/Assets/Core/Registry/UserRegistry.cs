using System.Linq;
using FullPotential.Core.Data;
using UnityEngine;

namespace FullPotential.Core.Registry
{
    public class UserRegistry
    {
        private readonly bool _isDebugBuild;
        private readonly string _persistentDataPath;

        public UserRegistry()
        {
            _isDebugBuild = Debug.isDebugBuild;
            _persistentDataPath = Application.persistentDataPath;
        }

        public string SignIn(string username, string password)
        {
            var token = string.IsNullOrWhiteSpace(username)
                ? SystemInfo.deviceUniqueIdentifier
                : username;

            return token;
        }

        public bool ValidateToken(string token)
        {
            return true;
        }

        public PlayerData Load(string token, string username, bool reduced)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
               username = token;
            }

            var filePath = GetPlayerSavePath(username);

            if (!System.IO.File.Exists(filePath))
            {
                return new PlayerData
                {
                    Options = new PlayerOptions(),
                    Inventory = new Inventory()
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
                throw new System.ArgumentException("No username supplied");
            }

            return _persistentDataPath + "/" + username + ".json";
        }

        private void StripExtraData(PlayerData playerData)
        {
            var equippedItemIds = playerData.Inventory.EquippedItems.Select(x => x.Value);

            playerData.Inventory.Accessories = playerData.Inventory.Accessories.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            playerData.Inventory.Armor = playerData.Inventory.Armor.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            playerData.Inventory.Loot = playerData.Inventory.Loot.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            playerData.Inventory.Spells = playerData.Inventory.Spells.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
            playerData.Inventory.Weapons = playerData.Inventory.Weapons.Where(x => equippedItemIds.Contains(x.Id)).ToArray();
        }

    }
}
