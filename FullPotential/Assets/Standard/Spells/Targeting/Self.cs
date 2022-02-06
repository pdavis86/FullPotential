using System;
using FullPotential.Api.Registry.Spells;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Standard.Spells.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Standard.Spells.Targeting
{
    public class Self : ISpellTargeting
    {
        public Guid TypeId => new Guid("b7cd2ff2-e054-4955-bde9-38b3b6b9a1bf");

        public string TypeName => nameof(Self);

        public bool HasShape => true;

        public bool IsContinuous => false;

        public GameObject SpawnGameObject(Spell activeSpell, Vector3 startPosition, Vector3 targetDirection, ulong senderClientId, bool isLeftHand = false, Transform parentTransform = null)
        {
            //todo: prefab should be an addressable
            var spellObject = UnityEngine.Object.Instantiate(GameManager.Instance.Prefabs.Combat.SpellSelf, startPosition, Quaternion.identity);

            var spellScript = spellObject.GetComponent<SpellSelfBehaviour>();
            spellScript.PlayerClientId = senderClientId;
            spellScript.SpellId = activeSpell.Id;
            spellScript.SpellDirection = targetDirection;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();

            return spellObject;
        }
    }
}
