using System;
using FullPotential.Api.Registry.Spells;
using FullPotential.Standard.Spells.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.Spells.Targeting
{
    public class Beam : ISpellTargeting
    {
        public Guid TypeId => new Guid("3c528d09-bd4f-4211-af17-d8721975fef1");

        public string TypeName => nameof(Beam);

        public bool HasShape => false;

        public bool IsContinuous => true;

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellBeam.prefab";

        public string IdlePrefabAddress => "Standard/Prefabs/Spells/SpellInHand.prefab";

        public void SetBehaviourVariables(GameObject gameObject, Spell activeSpell, Vector3 startPosition, Vector3 targetDirection, ulong senderClientId, bool isLeftHand = false, Transform parentTransform = null)
        {
            var spellScript = gameObject.GetComponent<SpellBeamBehaviour>();
            spellScript.SpellId = activeSpell.Id;
            spellScript.IsLeftHand = isLeftHand;
        }
        
    }
}
