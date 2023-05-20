using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Shapes;
using FullPotential.Api.Gameplay.Targeting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Core.GameManagement;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Core.Gameplay.Targeting
{
    public class ProjectileBehaviour : MonoBehaviour, ITargetingBehaviour
    {
        private ICombatService _combatService;
        private ITypeRegistry _typeRegistry;
        
        private bool _collisionDetected;
        public IFighter SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            Physics.IgnoreCollision(GetComponent<Collider>(), SourceFighter.GameObject.GetComponent<Collider>());

            var affectedByGravity = Consumer.Shape != null;

            var rigidBody = GetComponent<Rigidbody>();

            var shotDirection = Consumer.GetShotDirection(Direction);

            rigidBody.AddForce(20f * Consumer.GetProjectileSpeed() * shotDirection, ForceMode.VelocityChange);

            if (affectedByGravity)
            {
                rigidBody.useGravity = true;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (other.isTrigger)
            {
                return;
            }

            if (_collisionDetected)
            {
                return;
            }

            _collisionDetected = true;

            _combatService.ApplyEffects(SourceFighter, Consumer, other.gameObject, other.ClosestPointOnBounds(transform.position));

            SpawnShape(other.gameObject, other.ClosestPointOnBounds(transform.position));

            Consumer.StopStoppables();

            Destroy(gameObject);
        }

        private void SpawnShape(GameObject target, Vector3? position)
        {
            if (Consumer.Shape == null)
            {
                return;
            }

            if (!position.HasValue)
            {
                Debug.LogError("Position Vector3 cannot be null for spawning a shape");
                return;
            }

            Vector3 spawnPosition;
            if (!target.CompareTagAny(Tags.Player, Tags.Enemy))
            {
                spawnPosition = position.Value;
            }
            else
            {
                var pointUnderTarget = new Vector3(target.transform.position.x, -100, target.transform.position.z);
                var feetOfTarget = target.GetComponent<Collider>().ClosestPointOnBounds(pointUnderTarget);

                spawnPosition = Physics.Raycast(feetOfTarget, Vector3.down, out var hit)
                    ? hit.point
                    : position.Value;
            }

            if (Consumer.Shape is Wall)
            {
                var rotation = Quaternion.LookRotation(Direction);
                rotation.x = 0;
                rotation.z = 0;

                SpawnShapeGameObjects(GameManager.Instance.Prefabs.Shapes.Wall, spawnPosition, rotation);
            }
            else if (Consumer.Shape is Zone)
            {
                SpawnShapeGameObjects(GameManager.Instance.Prefabs.Shapes.Zone, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogError($"Unexpected shape for consumer {Consumer.Id} '{Consumer.Name}'");
            }
        }

        private void SpawnShapeGameObjects(GameObject prefab, Vector3 spawnPosition, Quaternion rotation)
        {
            var shapeGameObject = Instantiate(prefab, spawnPosition, rotation);

            shapeGameObject.NetworkSpawn();

            var shapeBehaviour = shapeGameObject.GetComponent<IShapeBehaviour>();
            shapeBehaviour.SourceFighter = SourceFighter;
            shapeBehaviour.Consumer = Consumer;
            shapeBehaviour.Direction = Direction;

            GameManager.Instance.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(spawnPosition, shapeGameObject.transform);

            if (!string.IsNullOrWhiteSpace(Consumer.ShapeVisuals?.PrefabAddress))
            {
                _typeRegistry.LoadAddessable(
                    Consumer.ShapeVisuals.PrefabAddress,
                    visualsPrefab =>
                    {
                        _combatService.SpawnConsumerVisuals(
                            visualsPrefab,
                            shapeGameObject.transform,
                            Consumer,
                            SourceFighter,
                            spawnPosition,
                            Direction,
                            null);
                    });
            }
            else if (shapeBehaviour.VisualsFallbackPrefab != null)
            {
                _combatService.SpawnConsumerVisuals(
                    shapeBehaviour.VisualsFallbackPrefab,
                    shapeGameObject.transform,
                    Consumer,
                    SourceFighter,
                    spawnPosition,
                    Direction,
                    null);
            }
        }
    }
}
