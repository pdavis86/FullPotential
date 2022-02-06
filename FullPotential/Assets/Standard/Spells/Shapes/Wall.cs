using System;
using FullPotential.Api.Registry.Spells;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Standard.Spells.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Standard.Spells.Shapes
{
    public class Wall : ISpellShape
    {
        public Guid TypeId => new Guid("4aad2866-5903-4b79-bda2-e3dcab920d9e");

        public string TypeName => nameof(Wall);

        public void SpawnGameObject(Spell activeSpell, Vector3 startPosition, Quaternion rotation, ulong senderClientId)
        {
            //todo: prefab should be an addressable
            var prefab = GameManager.Instance.Prefabs.Combat.SpellWall;

            var spellObject = UnityEngine.Object.Instantiate(prefab, startPosition, rotation);
            GameManager.Instance.SceneBehaviour.GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject);

            var spellScript = spellObject.GetComponent<SpellWallBehaviour>();
            spellScript.PlayerClientId = senderClientId;
            spellScript.SpellId = activeSpell.Id;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();
        }
    }
}
