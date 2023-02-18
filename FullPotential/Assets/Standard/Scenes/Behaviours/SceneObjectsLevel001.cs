using System.Collections.Generic;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Ioc;
using FullPotential.Api.Modding;
using FullPotential.Api.Scenes;
using FullPotential.Api.Spawning;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Standard.Enemies.Behaviours;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global

namespace FullPotential.Standard.Scenes.Behaviours
{
    public class SceneObjectsLevel001 : NetworkBehaviour, ISceneBehaviour
    {
#pragma warning disable 0649
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private float _spawnVariationMin = -4f;
        [SerializeField] private float _spawnVariationMax = 4f;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 0649

        private IGameManager _gameManager;
        private ISpawnService _spawnService;

        private List<Transform> _spawnPoints;
        private NetworkObject _enemyPrefabNetObj;
        private int _enemyCounter;

        [SerializeField] private SceneAttributes _attributes = new SceneAttributes();

        public SceneAttributes Attributes
        {
            get => _attributes;

            // ReSharper disable once UnusedMember.Global
            set => _attributes = value;
        }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _gameManager = DependenciesContext.Dependencies.GetService<IModHelper>().GetGameManager();
            _spawnService = DependenciesContext.Dependencies.GetService<ISpawnService>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCanvas).SetActive(true);

            if (!IsServer)
            {
                return;
            }

            SpawnEnemy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _enemyPrefabNetObj = _enemyPrefab.GetComponent<NetworkObject>();

            var spawnPointsParent = GameObjectHelper.GetObjectAtRoot(GameObjectNames.SpawnPoints).transform;
            _spawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in spawnPointsParent)
            {
                if (spawnPoint.gameObject.activeInHierarchy)
                {
                    _spawnPoints.Add(spawnPoint);
                }
            }

            var playerToken = _gameManager.GetLocalPlayerToken();
            var chosenSpawnPoint = GetSpawnPoint();
            HereAreMyJoiningDetailsServerRpc(playerToken, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HereAreMyJoiningDetailsServerRpc(string playerToken, Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
        {
            _gameManager.SpawnPlayerNetworkObject(playerToken, position, rotation, serverRpcParams);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void MakeAnnouncementClientRpc(string announcement, ClientRpcParams clientRpcParams)
        {
            if (announcement.IsNullOrWhiteSpace())
            {
                return;
            }

            _gameManager.GetUserInterface().HudOverlay.ShowAlert(announcement);
        }

        private void SpawnEnemy()
        {
            var chosenSpawnPoint = GetSpawnPoint();

            var enemyNetObj = Instantiate(_enemyPrefabNetObj, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);

            _spawnService.AdjustPositionToBeAboveGround(chosenSpawnPoint.Position, enemyNetObj.transform);

            enemyNetObj.Spawn(true);

            //Must re-parent after spawn
            enemyNetObj.transform.parent = transform;

            _enemyCounter++;
            enemyNetObj.GetComponent<EnemyState>().SetName("Enemy " + _enemyCounter);
        }

        public void HandleEnemyDeath()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                SpawnEnemy();
            }
        }

        public ISpawnService GetSpawnService()
        {
            return _spawnService;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public SpawnPoint GetSpawnPoint(GameObject gameObjectToSpawn = null)
        {
            var chosenSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            var spawnPosition = chosenSpawnPoint.position + new Vector3(
                Random.Range(_spawnVariationMin, _spawnVariationMax),
                0,
                Random.Range(_spawnVariationMin, _spawnVariationMax));

            if (gameObjectToSpawn != null)
            {
                _spawnService.AdjustPositionToBeAboveGround(spawnPosition, gameObjectToSpawn.transform);
                return new SpawnPoint
                {
                    Position = gameObjectToSpawn.transform.position,
                    Rotation = chosenSpawnPoint.rotation
                };
            }

            return new SpawnPoint
            {
                Position = spawnPosition,
                Rotation = chosenSpawnPoint.rotation
            };
        }

    }
}