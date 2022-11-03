using System;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.SpellsAndGadgets;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using FullPotential.Standard.SpellsAndGadgets.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.SpellsAndGadgets.Shapes
{
    public class Zone : IShape
    {
        public Guid TypeId => new Guid("142aeb3b-84b1-43c6-ae91-388b0901fa52");

        public string TypeName => nameof(Zone);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Zone.prefab";

        public void SpawnGameObject(SpellOrGadgetItemBase spellOrGadget, IFighter sourceFighter, Vector3 startPosition, Quaternion rotation)
        {
            var gameManager = ModHelper.GetGameManager();
            gameManager.GetService<ITypeRegistry>().LoadAddessable(
                spellOrGadget.Shape.PrefabAddress,
                prefab =>
                {
                    var spellObject = UnityEngine.Object.Instantiate(prefab, startPosition, rotation);
                    gameManager.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject.transform, false);

                    var spellScript = spellObject.GetComponent<SogZoneBehaviour>();
                    spellScript.SpellOrGadget = spellOrGadget;
                    spellScript.SourceFighter = sourceFighter;

                    spellObject.transform.parent = gameManager.GetSceneBehaviour().GetTransform();
                }
            );
        }
    }
}
