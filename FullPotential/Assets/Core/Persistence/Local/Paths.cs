using System;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public static class Paths
    {
        public static string GetCharacterSavePath(string username)
        {
            return GetBasePath(username) + ".json";
        }

        public static string GetInventorySavePath(string username)
        {
            return GetBasePath(username) + "_inventory.json";
        }

        private static string GetBasePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("No username supplied");
            }

            return Application.persistentDataPath + "/" + username;
        }
    }
}
