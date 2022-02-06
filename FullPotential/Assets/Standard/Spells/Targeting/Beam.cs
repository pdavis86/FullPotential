using System;
using FullPotential.Api.Registry.Spells;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Standard.Spells.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Standard.Spells.Targeting
{
    public class Beam : ISpellTargeting
    {
        public Guid TypeId => new Guid("3c528d09-bd4f-4211-af17-d8721975fef1");

        public string TypeName => nameof(Beam);

        public bool HasShape => false;

        public bool IsContinuous => true;

        public GameObject SpawnGameObject(Spell activeSpell, Vector3 startPosition, Vector3 targetDirection, ulong senderClientId, bool isLeftHand = false, Transform parentTransform = null)
        {
            //todo: prefab should be an addressable
            //NOTE: Can't parent to PlayerCamera otherwise it doesn't parent at all!
            var spellObject = UnityEngine.Object.Instantiate(
                GameManager.Instance.Prefabs.Combat.SpellBeam,
                startPosition,
                Quaternion.LookRotation(targetDirection)
            );

            var spellScript = spellObject.GetComponent<SpellBeamBehaviour>();
            spellScript.SpellId = activeSpell.Id;
            spellScript.IsLeftHand = isLeftHand;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = parentTransform;

            return spellObject;
        }
    }
}
