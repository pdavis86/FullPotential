using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Behaviours;

using Unity.Netcode;

using UnityEngine;

namespace FullPotential.Core.Persistence
{
    public class SaveManager : ISaveManager
    {
        private readonly Dictionary<string, List<ISaveable>> _queue = new Dictionary<string, List<ISaveable>>();

        private IDataSaver _dataSaver;
        private bool _isProcessingQueue;

        public SaveManager(IDataSaver dataSaver)
        {
            _dataSaver = dataSaver;
        }

        public void AddToQueue(string characterId, ISaveable saveable)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Tried saving when not on the server");
            }

            // todo: zzz v0.6 - set debug log level
            //Debug.Log($"Adding '{saveable.GetType().Name}' to save queue for '{characterId}'");

            if (!_queue.ContainsKey(characterId))
            {
                _queue.Add(characterId, new List<ISaveable> { saveable });
                return;
            }

            if (!_queue[characterId].Contains(saveable))
            {
                _queue[characterId].Add(saveable);
            }
        }

        public async UniTask ProcessQueueForCharacterIdAsync(string characterId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Tried saving when not on the server");
            }

            // todo: zzz v0.6 - set debug log level
            //Debug.Log($"Processing save queue for '{characterId}'");

            var tasks = GetUniTasksForCharacterId(characterId);

            _queue.Remove(characterId);

            await UniTask.WhenAll(tasks);
        }

        public async UniTask ProcessQueueAsync()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Tried saving when not on the server");
            }

            if (_isProcessingQueue)
            {
                Debug.LogWarning("Already processing the queue");
                return;
            }

            if (_queue.Count == 0)
            {
                // todo: zzz v0.6 - set debug log level
                //Debug.Log("Nothing in the save queue");
                return;
            }

            // todo: zzz v0.6 - set debug log level
            //Debug.Log("Processing save queue");

            _isProcessingQueue = true;

            try
            {
                var tasks = _queue.SelectMany(x => GetUniTasksForCharacterId(x.Key)).ToList();
                await UniTask.WhenAll(tasks);
                _queue.Clear();
            }
            finally
            {
                _isProcessingQueue = false;
            }
        }

        private IEnumerable<UniTask> GetUniTasksForCharacterId(string characterId)
        {
            if (!_queue.ContainsKey(characterId))
            {
                return Array.Empty<UniTask>();
            }

            var tasks = _queue[characterId].Select(x => SaveImmediatelyAsync(x, characterId)).ToList();

            return tasks;
        }

        private async UniTask SaveImmediatelyAsync(ISaveable saveable, string characterId)
        {
            if (!saveable.IsDirty)
            {
                // todo: zzz v0.6 - set debug log level
                //Debug.Log($"Did not save '{saveable.GetType().Name}' for user '{characterId}' because it was not dirty");
                return;
            }

            // todo: zzz v0.6 - set debug log level
            //Debug.Log($"Saving type '{saveable.GetType().Name}' for user {characterId}");

            try
            {
                if (saveable is IPlayerFighter playerFighter)
                {
                    await _dataSaver.SaveCharacterDataAsync(playerFighter.GetCharacterData());
                }

                if (saveable is InventoryBase inventory)
                {
                    await _dataSaver.SaveInventoryDataAsync(inventory.GetInventoryData());
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
