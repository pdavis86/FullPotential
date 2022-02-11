using System;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.Spells;
using FullPotential.Standard.Spells.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.Spells.Targeting
{
    public class Self : ISpellTargeting
    {
        public Guid TypeId => new Guid("b7cd2ff2-e054-4955-bde9-38b3b6b9a1bf");

        public string TypeName => nameof(Self);

        public bool HasShape => true;

        public bool IsContinuous => false;

        public bool IsParentedToCaster => false;

        public bool IsServerSideOnly => false;

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellSelf.prefab";

        public string IdlePrefabAddress => "Standard/Prefabs/Spells/SpellInHand.prefab";

        public void SetBehaviourVariables(GameObject gameObject, Spell spell, IPlayerStateBehaviour sourceStateBehaviour, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SpellSelfBehaviour>();
            spellScript.Spell = spell;
            spellScript.SourceStateBehaviour = sourceStateBehaviour;
            spellScript.ForwardDirection = forwardDirection;
        }

    }
}
