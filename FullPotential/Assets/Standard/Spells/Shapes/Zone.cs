using System;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Spells;
using FullPotential.Api.Utilities;
using FullPotential.Standard.Spells.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.Spells.Shapes
{
    public class Zone : ISpellShape
    {
        public Guid TypeId => new Guid("142aeb3b-84b1-43c6-ae91-388b0901fa52");

        public string TypeName => nameof(Zone);

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellZone.prefab";

        public void SpawnGameObject(Spell spell, IPlayerStateBehaviour sourceStateBehaviour, Vector3 startPosition, Quaternion rotation)
        {
            var gameManager = ModHelper.GetGameManager();
            gameManager.GetService<ITypeRegistry>().LoadAddessable(
                spell.Shape.PrefabAddress,
                prefab =>
                {
                    var spellObject = UnityEngine.Object.Instantiate(prefab, startPosition, rotation);
                    gameManager.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject.transform, false);

                    var spellScript = spellObject.GetComponent<SpellZoneBehaviour>();
                    spellScript.Spell = spell;
                    spellScript.SourceStateBehaviour = sourceStateBehaviour;

                    spellObject.transform.parent = gameManager.GetSceneBehaviour().GetTransform();
                }
            );
        }
    }
}
