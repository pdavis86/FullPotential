using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Behaviours;

using Unity.Netcode;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public class SaveManager : ISaveManager
    {
        private readonly Dictionary<string, List<ISaveable>> _queue = new Dictionary<string, List<ISaveable>>();

        private IPlayerManagement _playerManagement;
        private bool _isProcessingQueue;

        public SaveManager(IPlayerManagement playerManagement)
        {
            _playerManagement = playerManagement;
        }

        public void AddToQueue(string username, ISaveable saveable)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Tried saving when not on the server");
                return;
            }

            if (!_queue.ContainsKey(username))
            {
                _queue.Add(username, new List<ISaveable> { saveable });
                return;
            }

            if (!_queue[username].Contains(saveable))
            {
                _queue[username].Add(saveable);
            }
        }

        public async UniTask ProcessQueueForUsernameAsync(string username)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Tried saving when not on the server");
                return;
            }

            // todo: remove debugging
            //Debug.Log($"Processing save queue for {username}");

            var tasks = GetUniTasksForusername(username);

            _queue.Remove(username);

            await UniTask.WhenAll(tasks);
        }

        public async UniTask ProcessQueueAsync()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Tried saving when not on the server");
                return;
            }

            if (_isProcessingQueue)
            {
                Debug.LogWarning("Already processing the queue");
                return;
            }

            if (_queue.Count == 0)
            {
                return;
            }

            // todo: remove debugging
            //Debug.Log("Processing save queue");

            _isProcessingQueue = true;

            try
            {
                var tasks = _queue.SelectMany(x => GetUniTasksForusername(x.Key)).ToList();
                await UniTask.WhenAll(tasks);
                _queue.Clear();
            }
            finally
            {
                _isProcessingQueue = false;
            }
        }

        private IEnumerable<UniTask> GetUniTasksForusername(string username)
        {
            if (!_queue.ContainsKey(username))
            {
                return Array.Empty<UniTask>();
            }

            var tasks = _queue[username].Select(x => SaveImmediatelyAsync(x, username)).ToList();

            return tasks;
        }

        private async UniTask SaveImmediatelyAsync(ISaveable saveable, string username)
        {
            if (!saveable.IsDirty)
            {
                Debug.LogWarning($"Did not save {saveable.GetType().Name} for username '{username}' because it was not dirty");
                return;
            }

            // todo: remove debugging
            Debug.Log($"Saving type {saveable.GetType().Name} for user {username}");

            if (saveable is IPlayerFighter playerFighter)
            {
                await _playerManagement.SavePlayerDataAsync(playerFighter.GetPlayerData());
                playerFighter.IsDirty = false;
            }

            if (saveable is InventoryBase inventory)
            {
                await _playerManagement.SaveInventoryChangesAsync(inventory.GetInventoryChanges());
                inventory.IsDirty = false;
            }
        }
    }
}
