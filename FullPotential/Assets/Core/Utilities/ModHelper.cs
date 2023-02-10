using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using FullPotential.Core.GameManagement;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Utilities
{
    public class ModHelper : IModHelper
    {
        public IGameManager GetGameManager()
        {
            return GameManager.Instance;
        }

        public void SpawnShapeGameObject<T>(SpellOrGadgetItemBase spellOrGadget, IFighter sourceFighter, Vector3 startPosition, Quaternion rotation) 
            where T : IShapeBehaviour
        {
            var gameManager = GetGameManager();
            var typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            var spawnService = gameManager.GetSceneBehaviour().GetSpawnService();
            var sceneBehaviour = gameManager.GetSceneBehaviour();

            typeRegistry.LoadAddessable(
                spellOrGadget.Shape.PrefabAddress,
                prefab =>
                {
                    var sog = Object.Instantiate(prefab, startPosition, rotation);
                    spawnService.AdjustPositionToBeAboveGround(startPosition, sog.transform, false);

                    var behaviourScript = sog.GetComponent<T>();
                    behaviourScript.SpellOrGadget = spellOrGadget;
                    behaviourScript.SourceFighter = sourceFighter;

                    sog.transform.parent = sceneBehaviour.GetTransform();
                }
            );
        }
    }

}
