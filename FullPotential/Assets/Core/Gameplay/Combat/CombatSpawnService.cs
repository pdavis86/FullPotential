using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Items;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;

using UnityEngine;

namespace FullPotential.Core.Gameplay.Combat
{
    public class CombatSpawnService
    {
        private readonly IAuditor _logger;
        private readonly ITypeRegistry _typeRegistry;

        public CombatSpawnService(IAuditorFactory auditorFactory, ITypeRegistry typeRegistry)
        {
            _logger = auditorFactory.Create(this);
            _typeRegistry = typeRegistry;
        }

        public void SpawnTargetingGameObject(FighterBase sourceFighter, IConsumer consumer, Vector3 startPosition, Vector3 direction)
        {
            if (consumer.TargetingType.NetworkPrefabAddress.IsNullOrWhiteSpace())
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                consumer.TargetingType.NetworkPrefabAddress,
                prefab =>
                {
                    var targetingGameObject = Object.Instantiate(prefab, startPosition, Quaternion.identity);

                    var targetingBehaviour = targetingGameObject.GetComponent<ITargetingBehaviour>();
                    targetingBehaviour.SourceFighter = sourceFighter;
                    targetingBehaviour.Consumer = consumer;
                    targetingBehaviour.Direction = direction;

                    targetingGameObject.NetworkSpawn(_logger);

                    if (consumer.TargetingType.IsContinuous)
                    {
                        targetingGameObject.transform.parent = sourceFighter.Transform;
                    }

                    consumer.Stoppables.Add(new DestroyStoppable(targetingGameObject));
                });
        }

        public void SpawnShapeGameObject(FighterBase sourceFighter, IConsumer consumer, GameObject target, Vector3 fallbackPosition, Vector3 lookDirection)
        {
            if (consumer.ShapeType == null)
            {
                _logger.Debug("Cancelling SpawnShapeGameObject because there was no Shape");
                return;
            }

            Vector3 spawnPosition;
            if (target == null || !target.CompareTagAny(Tags.Player, Tags.Enemy))
            {
                spawnPosition = fallbackPosition;
            }
            else
            {
                var pointUnderTarget = new Vector3(target.transform.position.x, -100, target.transform.position.z);
                var feetOfTarget = target.GetComponent<Collider>().ClosestPointOnBounds(pointUnderTarget);

                spawnPosition = Physics.Raycast(feetOfTarget, Vector3.down, out var hit)
                    ? hit.point
                    : fallbackPosition;
            }

            var rotation = Quaternion.LookRotation(lookDirection);
            rotation.x = 0;
            rotation.z = 0;

            _typeRegistry.LoadAddessable<GameObject>(
                consumer.ShapeType.NetworkPrefabAddress,
                prefab =>
                {
                    var shapeGameObject = Object.Instantiate(prefab, spawnPosition, rotation);
                    var sceneService = GameManager.Instance.GetSceneBehaviour().GetSceneService();

                    shapeGameObject.transform.position = sceneService.GetHeightAdjustedPosition(spawnPosition, shapeGameObject);

                    shapeGameObject.layer = LayerMask.NameToLayer(Layers.NonSolid);

                    var shapeBehaviour = shapeGameObject.GetComponent<IShapeBehaviour>();
                    shapeBehaviour.SourceFighter = sourceFighter;
                    shapeBehaviour.Consumer = consumer;
                    shapeBehaviour.Direction = lookDirection;

                    shapeGameObject.NetworkSpawn(_logger);
                });
        }
    }
}
