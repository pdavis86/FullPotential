using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FullPotential.Api.Data;
using FullPotential.Api.Persistence;
using FullPotential.Core.Player;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Core.Persistence
{
    public class PersistenceService : IPersistenceService
    {
        private readonly IUserRepository _userRepository;
        private readonly List<string> _asapSaveUsernames  = new List<string>();

        private bool _isSaving;

        public PersistenceService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void SaveBatchPlayerData(Dictionary<ulong, string> clientIdToUsernameMapping, bool allData = false)
        {
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
                var tasks = playerDataCollection.Select(x => Task.Run(() => SavePlayerData(x)));
                Task.Run(async () => await Task.WhenAll(tasks))
                    .GetAwaiter()
                    .GetResult();
            }
            finally
            {
                _isSaving = false;
            }
        }

        public void QueueAsapSave(string username)
        {
            if (!_asapSaveUsernames.Contains(username))
            {
                _asapSaveUsernames.Add(username);
            }
        }

        public void SavePlayerData(PlayerData playerData)
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

            _userRepository.Save(playerData);

            _asapSaveUsernames.Remove(playerData.Username);
        }
    }
}
