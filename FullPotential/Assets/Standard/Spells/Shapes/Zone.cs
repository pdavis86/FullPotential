using System;
using FullPotential.Api;
using FullPotential.Api.Registry.Spells;
using FullPotential.Standard.Spells.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Standard.Spells.Shapes
{
    public class Zone : ISpellShape
    {
        public Guid TypeId => new Guid("142aeb3b-84b1-43c6-ae91-388b0901fa52");

        public string TypeName => nameof(Zone);

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellZone.prefab";

        public void SpawnGameObject(Spell activeSpell, Vector3 startPosition, Quaternion rotation, ulong senderClientId)
        {
            var gameManager = ModHelper.GetGameManager();
            gameManager.TypeRegistry.LoadAddessable(
                activeSpell.Targeting.PrefabAddress,
                prefab =>
                {
                    var spellObject = UnityEngine.Object.Instantiate(prefab, startPosition, rotation);
                    gameManager.SceneBehaviour.GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject, false);

                    var spellScript = spellObject.GetComponent<SpellZoneBehaviour>();
                    spellScript.PlayerClientId = senderClientId;
                    spellScript.SpellId = activeSpell.Id;

                    spellObject.GetComponent<NetworkObject>().Spawn(true);

                    spellObject.transform.parent = gameManager.SceneBehaviour.GetTransform();
                }
            );
        }
    }
}
