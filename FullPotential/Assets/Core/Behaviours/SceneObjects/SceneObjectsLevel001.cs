using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Behaviours.SceneObjects
{
    public class SceneObjectsLevel001 : NetworkBehaviour
    {
        private NetworkObject _playerPrefabNetObj;
        private List<Transform> _spawnPoints;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _playerPrefabNetObj = GameManager.Instance.Prefabs.Player.GetComponent<NetworkObject>();

            var spawnPointsParent = Helpers.UnityHelper.GetObjectAtRoot(GameManager.NameSpawnPoints).transform;
            _spawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in spawnPointsParent)
            {
                if (spawnPoint.gameObject.activeInHierarchy)
                {
                    _spawnPoints.Add(spawnPoint);
                }
            }

            HeresMyJoiningDetailsServerRpc(GameManager.Instance.DataStore.PlayerToken);
        }

        [ServerRpc(RequireOwnership = false)]
        public void HeresMyJoiningDetailsServerRpc(string playerToken, ServerRpcParams serverRpcParams = default)
        {
            SpawnPlayer(serverRpcParams.Receive.SenderClientId, playerToken);
        }

        private void SpawnPlayer(ulong clientId, string playerToken)
        {
            var chosenSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            var playerNetObj = Instantiate(_playerPrefabNetObj, chosenSpawnPoint.position, chosenSpawnPoint.rotation);

            var playerState = playerNetObj.GetComponent<PlayerState>();
            playerState.PlayerToken = playerToken;

            playerNetObj.SpawnAsPlayerObject(clientId);
        }

    }
}