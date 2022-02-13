using System;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Spells;
using FullPotential.Api.Utilities;
using FullPotential.Standard.Spells.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.Spells.Shapes
{
    public class Wall : ISpellShape
    {
        public Guid TypeId => new Guid("4aad2866-5903-4b79-bda2-e3dcab920d9e");

        public string TypeName => nameof(Wall);

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellWall.prefab";

        public void SpawnGameObject(Spell spell, IPlayerStateBehaviour sourceStateBehaviour, Vector3 startPosition, Quaternion rotation)
        {
            var gameManager = ModHelper.GetGameManager();
            gameManager.GetService<ITypeRegistry>().LoadAddessable(
                spell.Shape.PrefabAddress,
                prefab =>
                {
                    var spellObject = UnityEngine.Object.Instantiate(prefab, startPosition, rotation);
                    gameManager.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject.transform);

                    var spellScript = spellObject.GetComponent<SpellWallBehaviour>();
                    spellScript.Spell = spell;
                    spellScript.SourceStateBehaviour = sourceStateBehaviour;

                    spellObject.transform.parent = gameManager.GetSceneBehaviour().GetTransform();
                }
            );
        }

    }
}
