using Unity.Netcode;
using System.Collections.Generic;
using FullPotential.Api.Behaviours;
using UnityEngine;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Helpers;
using FullPotential.Core.Spawning;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Behaviours.SceneObjects
{
    public class SceneObjectsLevel001 : NetworkBehaviour, ISceneBehaviour
    {
        public GameObject EnemyPrefab;

        [SerializeField] private readonly Vector2 _spawnVariation = new Vector2(-4f, 4f);

        private List<Transform> _spawnPoints;
        private NetworkObject _playerPrefabNetObj;
        private NetworkObject _enemyPrefabNetObj;
        private SpawnService _spawnService;

        private void Awake()
        {
            _spawnService = new SpawnService();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _playerPrefabNetObj = GameManager.Instance.Prefabs.Player.GetComponent<NetworkObject>();
            _enemyPrefabNetObj = EnemyPrefab.GetComponent<NetworkObject>();

            var spawnPointsParent = UnityHelper.GetObjectAtRoot(Constants.GameObjectNames.SpawnPoints).transform;
            _spawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in spawnPointsParent)
            {
                if (spawnPoint.gameObject.activeInHierarchy)
                {
                    _spawnPoints.Add(spawnPoint);
                }
            }

            HereAreMyJoiningDetailsServerRpc(GameManager.Instance.DataStore.PlayerToken);
        }

        [ServerRpc(RequireOwnership = false)]
        public void HereAreMyJoiningDetailsServerRpc(string playerToken, ServerRpcParams serverRpcParams = default)
        {
            var chosenSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            var playerNetObj = Instantiate(_playerPrefabNetObj, chosenSpawnPoint.position, chosenSpawnPoint.rotation);

            var playerState = playerNetObj.GetComponent<PlayerState>();
            playerState.PlayerToken = playerToken;

            playerNetObj.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);
        }

        public void SpawnEnemy()
        {
            var chosenSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            var spawnPosition = chosenSpawnPoint.position + new Vector3(
                Random.Range(_spawnVariation.x, _spawnVariation.y),
                0,
                Random.Range(_spawnVariation.x, _spawnVariation.y));

            var enemyNetObj = Instantiate(_enemyPrefabNetObj, spawnPosition, chosenSpawnPoint.rotation, transform);

            _spawnService.AdjustPositionToBeAboveGround(spawnPosition, enemyNetObj.gameObject);

            enemyNetObj.Spawn(true);
        }

        public void OnEnemyDeath()
        {
            SpawnEnemy();
        }

        public SpawnService GetSpawnService()
        {
            return _spawnService;
        }

        public Transform GetTransform()
        {
            return transform;
        }
    }
}