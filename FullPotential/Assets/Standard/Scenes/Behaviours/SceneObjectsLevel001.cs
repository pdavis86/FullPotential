using System.Collections.Generic;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Ioc;
using FullPotential.Api.Modding;
using FullPotential.Api.Scenes;
using FullPotential.Api.Unity.Services;
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
        private ISceneService _sceneService;
        private IUnityHelperUtilities _unityHelperUtilities;

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
            _sceneService = DependenciesContext.Dependencies.GetService<ISceneService>();
            _unityHelperUtilities = DependenciesContext.Dependencies.GetService<IUnityHelperUtilities>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _unityHelperUtilities.GetObjectAtRoot(GameObjectNames.SceneCanvas).SetActive(true);

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

            var spawnPointsParent = _unityHelperUtilities.GetObjectAtRoot(GameObjectNames.SpawnPoints).transform;
            _spawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in spawnPointsParent)
            {
                if (spawnPoint.gameObject.activeInHierarchy)
                {
                    _spawnPoints.Add(spawnPoint);
                }
            }

            var chosenSpawnPoint = GetSpawnPoint();
            HereAreMyJoiningDetailsServerRpc(chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HereAreMyJoiningDetailsServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
        {
            _gameManager.SpawnPlayerNetworkObject(position, rotation, serverRpcParams);
        }

        private void SpawnEnemy()
        {
            var chosenSpawnPoint = GetSpawnPoint();

            var enemyNetObj = Instantiate(_enemyPrefabNetObj, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);

            enemyNetObj.transform.position = _sceneService.GetHeightAdjustedPosition(chosenSpawnPoint.Position, enemyNetObj.GetComponent<Collider>());

            enemyNetObj.Spawn(true);

            enemyNetObj.transform.parent = transform;

            _enemyCounter++;

            var enemyState = enemyNetObj.GetComponent<EnemyFighter>();
            enemyState.SetName("Enemy " + _enemyCounter);
        }

        public void HandleEnemyDeath()
        {
            if (IsServer)
            {
                SpawnEnemy();
            }
        }

        public ISceneService GetSceneService()
        {
            return _sceneService;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public SpawnPoint GetSpawnPoint()
        {
            var chosenSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            var spawnPosition = chosenSpawnPoint.position + new Vector3(
                Random.Range(_spawnVariationMin, _spawnVariationMax),
                0,
                Random.Range(_spawnVariationMin, _spawnVariationMax));

            return new SpawnPoint
            {
                Position = spawnPosition,
                Rotation = chosenSpawnPoint.rotation
            };
        }

    }
}