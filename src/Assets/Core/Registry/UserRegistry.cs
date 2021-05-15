using Assets.Core.Data;
using UnityEngine;

namespace Assets.Core.Registry
{
    public class UserRegistry
    {
        public string Token { get; private set; }

        public void SignIn(string username, string password)
        {
            //todo: implement UserRegistry.SignIn()
            Token = username;
        }

        private string GetPlayerSavePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                //todo: remove
                username = SystemInfo.deviceUniqueIdentifier;

                //todo: throw new System.ArgumentException($"No username supplied to {nameof(GetPlayerSavePath)}()");
            }

            //todo: username must be a filesystem-safe string
            return System.IO.Path.Combine(Application.persistentDataPath, username + ".json");
        }

        public PlayerData Load(string token)
        {
            var username = token;

            var filePath = GetPlayerSavePath(username);

            if (!System.IO.File.Exists(filePath))
            {
                return new PlayerData
                {
                    Username = username
                };
            }

            var loadJson = System.IO.File.ReadAllText(filePath);
            return JsonUtility.FromJson<PlayerData>(loadJson);
        }

        public void Save(PlayerData playerData)
        {
            var prettyPrint = Debug.isDebugBuild;
            var saveJson = JsonUtility.ToJson(playerData, prettyPrint);
            System.IO.File.WriteAllText(GetPlayerSavePath(playerData.Username), saveJson);
        }

    }
}
