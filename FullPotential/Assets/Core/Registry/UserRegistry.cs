using FullPotential.Core.Data;
using UnityEngine;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace FullPotential.Core.Registry
{
    public static class UserRegistry
    {
        public static string SignIn(string username, string password)
        {
            var token = string.IsNullOrWhiteSpace(username)
                ? SystemInfo.deviceUniqueIdentifier
                : username;

            return token;
        }

        public static bool ValidateToken(string token)
        {
            return true;
        }

        public static PlayerData Load(string token, string username)
        {
            var filePath = string.IsNullOrWhiteSpace(token)
                ? GetPlayerSavePath(username)
                : GetPlayerSavePath(token);

            if (!System.IO.File.Exists(filePath))
            {
                return new PlayerData
                {
                    Username = username,
                    Options = new PlayerOptions()
                };
            }

            var loadJson = System.IO.File.ReadAllText(filePath);
            var playerData = JsonUtility.FromJson<PlayerData>(loadJson);

            if (string.IsNullOrWhiteSpace(playerData.Username))
            {
                playerData.Username = username;
            }

            if (playerData.Options == null)
            {
                playerData.Options = new PlayerOptions();
            }

            return playerData;
        }

        public static void Save(PlayerData playerData)
        {
            var prettyPrint = Debug.isDebugBuild;
            var saveJson = JsonUtility.ToJson(playerData, prettyPrint);
            System.IO.File.WriteAllText(GetPlayerSavePath(playerData.Username), saveJson);
        }

        private static string GetPlayerSavePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new System.ArgumentException("No username supplied");
            }

            return Application.persistentDataPath + "/" + username + ".json";
        }

    }
}
