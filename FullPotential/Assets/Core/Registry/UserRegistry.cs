using FullPotential.Assets.Core.Data;
using UnityEngine;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace FullPotential.Assets.Core.Registry
{
    public static class UserRegistry
    {
        public static string SignIn(string username, string password)
        {
            //todo: implement UserRegistry.SignIn(), make a unity web request to get the token

            //todo: remove once implemented properly
            if (string.IsNullOrWhiteSpace(username))
            {
                username = SystemInfo.deviceUniqueIdentifier;
            }

            return username;
        }

        private static string GetPlayerSavePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new System.ArgumentException($"No username supplied to {nameof(GetPlayerSavePath)}()");
            }

            //todo: username must be a filesystem-safe string
            return System.IO.Path.Combine(Application.persistentDataPath, username + ".json");
        }

        public static PlayerData Load(string token)
        {
            //todo: implement this
            return LoadFromUsername(token);
        }

        public static PlayerData LoadFromUsername(string username)
        {
            var filePath = GetPlayerSavePath(username);

            if (!System.IO.File.Exists(filePath))
            {
                return new PlayerData
                {
                    Username = username,
                    Options = new Options()
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
                playerData.Options = new Options();
            }

            return playerData;
        }

        public static void Save(PlayerData playerData)
        {
            var prettyPrint = Debug.isDebugBuild;
            var saveJson = JsonUtility.ToJson(playerData, prettyPrint);
            System.IO.File.WriteAllText(GetPlayerSavePath(playerData.Username), saveJson);
        }

    }
}
