using System;
using FullPotential.Api;
using FullPotential.Api.Registry.Spells;
using FullPotential.Standard.Spells.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Standard.Spells.Shapes
{
    public class Wall : ISpellShape
    {
        public Guid TypeId => new Guid("4aad2866-5903-4b79-bda2-e3dcab920d9e");

        public string TypeName => nameof(Wall);

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellWall.prefab";

        public void SpawnGameObject(Spell activeSpell, Vector3 startPosition, Quaternion rotation, ulong senderClientId)
        {
            var gameManager = ModHelper.GetGameManager();
            gameManager.TypeRegistry.LoadAddessable(
                activeSpell.Targeting.PrefabAddress,
                prefab =>
                {
                    var spellObject = UnityEngine.Object.Instantiate(prefab, startPosition, rotation);
                    gameManager.SceneBehaviour.GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject);

                    var spellScript = spellObject.GetComponent<SpellWallBehaviour>();
                    spellScript.PlayerClientId = senderClientId;
                    spellScript.SpellId = activeSpell.Id;

                    spellObject.GetComponent<NetworkObject>().Spawn(true);

                    spellObject.transform.parent = gameManager.SceneBehaviour.GetTransform();
                }
            );
        }

    }
}
