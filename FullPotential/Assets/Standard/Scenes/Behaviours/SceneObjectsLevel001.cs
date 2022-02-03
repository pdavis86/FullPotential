using System.Collections.Generic;
using FullPotential.Api.Scenes;
using FullPotential.Api.Spawning;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.Ui;
using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using FullPotential.Core.Spawning;
using FullPotential.Standard.Enemies.Behaviours;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global

namespace FullPotential.Standard.Scenes.Behaviours
{
    public class SceneObjectsLevel001 : NetworkBehaviour, ISceneBehaviour
    {
        // ReSharper disable once UnassignedField.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public GameObject EnemyPrefab;

        [SerializeField] private readonly Vector2 _spawnVariation = new Vector2(-4f, 4f);

        private List<Transform> _spawnPoints;
        private NetworkObject _playerPrefabNetObj;
        private NetworkObject _enemyPrefabNetObj;
        private ISpawnService _spawnService;
        private int _enemyCounter;

        [SerializeField] private SceneAttributes _attributes = new SceneAttributes();
        [SerializeField]
        public SceneAttributes Attributes
        {
            get => _attributes;
            // ReSharper disable once UnusedMember.Global
            set => _attributes = value;
        }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _spawnService = new SpawnService();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            GameObjectHelper.GetObjectAtRoot(Core.Constants.GameObjectNames.SceneCanvas).SetActive(true);

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

            var spawnPointsParent = GameObjectHelper.GetObjectAtRoot(Core.Constants.GameObjectNames.SpawnPoints).transform;
            _spawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in spawnPointsParent)
            {
                if (spawnPoint.gameObject.activeInHierarchy)
                {
                    _spawnPoints.Add(spawnPoint);
                }
            }

            HereAreMyJoiningDetailsServerRpc(GameManager.Instance.LocalGameDataStore.PlayerToken);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HereAreMyJoiningDetailsServerRpc(string playerToken, ServerRpcParams serverRpcParams = default)
        {
            var chosenSpawnPoint = GetSpawnPoint();
            var playerNetObj = Instantiate(_playerPrefabNetObj, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);

            var playerState = playerNetObj.GetComponent<PlayerState>();
            playerState.PlayerToken = playerToken;

            playerNetObj.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);

            _spawnService.AdjustPositionToBeAboveGround(chosenSpawnPoint.Position, playerNetObj.gameObject);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void MakeAnnouncementClientRpc(string announcement, ClientRpcParams clientRpcParams)
        {
            if (announcement.IsNullOrWhiteSpace())
            {
                return;
            }

            GameManager.Instance.MainCanvasObjects.Hud.GetComponent<Hud>().ShowAlert(announcement);
        }

        private void SpawnEnemy()
        {
            var chosenSpawnPoint = GetSpawnPoint();

            var enemyNetObj = Instantiate(_enemyPrefabNetObj, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);

            _spawnService.AdjustPositionToBeAboveGround(chosenSpawnPoint.Position, enemyNetObj.gameObject);

            _enemyCounter++;

            enemyNetObj.Spawn(true);

            enemyNetObj.transform.parent = transform;

            enemyNetObj.GetComponent<EnemyState>().EnemyName.Value = "Enemy " + _enemyCounter;
        }

        public void HandleEnemyDeath()
        {
            SpawnEnemy();
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