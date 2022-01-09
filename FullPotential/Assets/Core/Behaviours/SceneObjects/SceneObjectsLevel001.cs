using Unity.Netcode;
using System.Collections.Generic;
using FullPotential.Api.Behaviours;
using FullPotential.Api.Data;
using UnityEngine;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Helpers;
using FullPotential.Core.Spawning;
using TMPro;
using Random = UnityEngine.Random;

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
        private int _enemyCounter;

        [SerializeField] private SceneAttributes _attributes = new SceneAttributes();
        [SerializeField]
        public SceneAttributes Attributes
        {
            get => _attributes;
            set => _attributes = value;
        }

        private void Awake()
        {
            _spawnService = new SpawnService();
        }

        private void Start()
        {
            if (IsClient)
            {
                Camera.main.fieldOfView = GameManager.Instance.AppOptions.FieldOfView;
            }

            if (!IsServer)
            {
                return;
            }

            SpawnEnemy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _playerPrefabNetObj = GameManager.Instance.Prefabs.Player.GetComponent<NetworkObject>();
            _enemyPrefabNetObj = EnemyPrefab.GetComponent<NetworkObject>();

            var spawnPointsParent = GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.SpawnPoints).transform;
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
            var chosenSpawnPoint = GetSpawnPoint();
            var playerNetObj = Instantiate(_playerPrefabNetObj, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);

            var playerState = playerNetObj.GetComponent<PlayerState>();
            playerState.PlayerToken = playerToken;

            playerNetObj.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);

            _spawnService.AdjustPositionToBeAboveGround(chosenSpawnPoint.Position, playerNetObj.gameObject);
        }

        public void SpawnEnemy()
        {
            var chosenSpawnPoint = GetSpawnPoint();

            var enemyNetObj = Instantiate(_enemyPrefabNetObj, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);

            _spawnService.AdjustPositionToBeAboveGround(chosenSpawnPoint.Position, enemyNetObj.gameObject);

            _enemyCounter++;

            //todo: do this better
            enemyNetObj.gameObject.transform.Find("Graphics").Find("Canvas").Find("NameTag").GetComponent<TextMeshProUGUI>().text = "Enemy " + _enemyCounter;

            enemyNetObj.Spawn(true);

            enemyNetObj.transform.parent = transform;
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

        public SpawnPoint GetSpawnPoint(GameObject gameObjectToSpawn = null)
        {
            var chosenSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            var spawnPosition = chosenSpawnPoint.position + new Vector3(
                Random.Range(_spawnVariation.x, _spawnVariation.y),
                0,
                Random.Range(_spawnVariation.x, _spawnVariation.y));

            if (gameObjectToSpawn != null)
            {
                _spawnService.AdjustPositionToBeAboveGround(spawnPosition, gameObjectToSpawn);
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