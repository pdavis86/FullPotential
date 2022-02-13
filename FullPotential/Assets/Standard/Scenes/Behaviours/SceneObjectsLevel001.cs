using System.Collections.Generic;
using FullPotential.Api.Scenes;
using FullPotential.Api.Spawning;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement.Constants;
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
            _spawnService = ModHelper.GetGameManager().GetService<ISpawnService>();

            //Cannot currently add prefabs at runtime
            //https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1360
            //So enemy prefab is part of FullPotential.Core
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

            _enemyPrefabNetObj = EnemyPrefab.GetComponent<NetworkObject>();

            var spawnPointsParent = GameObjectHelper.GetObjectAtRoot(GameObjectNames.SpawnPoints).transform;
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

            ModHelper.GetGameManager().GetUserInterface().HudOverlay.ShowAlert(announcement);
        }

        private void SpawnEnemy()
        {
            var chosenSpawnPoint = GetSpawnPoint();

            var enemyNetObj = Instantiate(_enemyPrefabNetObj, chosenSpawnPoint.Position, chosenSpawnPoint.Rotation);

            _spawnService.AdjustPositionToBeAboveGround(chosenSpawnPoint.Position, enemyNetObj.transform);

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