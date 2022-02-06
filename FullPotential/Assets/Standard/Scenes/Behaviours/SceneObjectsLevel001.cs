using System.Collections.Generic;
using FullPotential.Api;
using FullPotential.Api.Extensions;
using FullPotential.Api.Helpers;
using FullPotential.Api.Scenes;
using FullPotential.Api.Spawning;
using FullPotential.Standard.Enemies.Behaviours;
using Unity.Netcode;
using UnityEngine;

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

            if (!IsServer)
            {
                return;
            }

            SpawnEnemy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

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

            var playerToken = ModHelper.GetGameManager().GetLocalPlayerToken();
            var chosenSpawnPoint = GetSpawnPoint();
            HereAreMyJoiningDetailsServerRpc(playerToken, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HereAreMyJoiningDetailsServerRpc(string playerToken, Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
        {
            ModHelper.GetGameManager().SpawnPlayerNetworkObject(playerToken, position, rotation, serverRpcParams);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void MakeAnnouncementClientRpc(string announcement, ClientRpcParams clientRpcParams)
        {
            if (announcement.IsNullOrWhiteSpace())
            {
                return;
            }

            ModHelper.GetGameManager().UserInterface.HudOverlay.ShowAlert(announcement);
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