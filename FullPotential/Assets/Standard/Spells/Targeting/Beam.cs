using System;
using FullPotential.Api.Gameplay;
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

        public bool IsParentedToCaster => true;

        public bool IsServerSideOnly => false;

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellBeam.prefab";

        public string IdlePrefabAddress => "Standard/Prefabs/Spells/SpellInHand.prefab";

        public void SetBehaviourVariables(GameObject gameObject, Spell spell, IPlayerStateBehaviour sourceStateBehaviour, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SpellBeamBehaviour>();
            spellScript.Spell = spell;
            spellScript.SourceStateBehaviour = sourceStateBehaviour;
            spellScript.IsLeftHand = isLeftHand;
        }

    }
}
