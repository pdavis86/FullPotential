using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
using FullPotential.Api.Registry.Consumers;
using FullPotential.Core.GameManagement;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Modding
{
    public class ModHelper : IModHelper
    {
        public IGameManager GetGameManager()
        {
            return GameManager.Instance;
        }

        public void SpawnShapeGameObject<T>(Consumer consumer, IFighter sourceFighter, Vector3 startPosition, Quaternion rotation) 
            where T : IShapeBehaviour
        {
            var gameManager = GetGameManager();
            var typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            var spawnService = gameManager.GetSceneBehaviour().GetSpawnService();
            var sceneBehaviour = gameManager.GetSceneBehaviour();

            typeRegistry.LoadAddessable(
                consumer.Shape.PrefabAddress,
                prefab =>
                {
                    var consumerGameObject = Object.Instantiate(prefab, startPosition, rotation);
                    spawnService.AdjustPositionToBeAboveGround(startPosition, consumerGameObject.transform, false);

                    var behaviourScript = consumerGameObject.GetComponent<T>();
                    behaviourScript.Consumer = consumer;
                    behaviourScript.SourceFighter = sourceFighter;

                    consumerGameObject.transform.parent = sceneBehaviour.GetTransform();
                }
            );
        }
    }

}
