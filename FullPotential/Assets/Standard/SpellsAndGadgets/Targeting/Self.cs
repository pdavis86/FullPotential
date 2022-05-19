using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Standard.SpellsAndGadgets.Behaviours;
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

        public void SetBehaviourVariables(GameObject gameObject, SpellOrGadgetItemBase spellOrGadget, IFighter sourceFighter, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SogSelfBehaviour>();
            spellScript.SpellOrGadget = spellOrGadget;
            spellScript.SourceFighter = sourceFighter;
            spellScript.ForwardDirection = forwardDirection;
        }

    }
}
