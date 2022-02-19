using System;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Standard.Spells.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.SpellsAndGadgets.Targeting
{
    public class Self : ITargeting
    {
        public Guid TypeId => new Guid("b7cd2ff2-e054-4955-bde9-38b3b6b9a1bf");

        public string TypeName => nameof(Self);

        public bool HasShape => true;

        public bool IsContinuous => false;

        public bool IsParentedToSource => false;

        public bool IsServerSideOnly => false;

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Self.prefab";

        public void SetBehaviourVariables(GameObject gameObject, SpellOrGadgetItemBase spellOrGadget, IPlayerStateBehaviour sourceStateBehaviour, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SpellSelfBehaviour>();
            spellScript.SpellOrGadget = spellOrGadget;
            spellScript.SourceStateBehaviour = sourceStateBehaviour;
            spellScript.ForwardDirection = forwardDirection;
        }

    }
}
