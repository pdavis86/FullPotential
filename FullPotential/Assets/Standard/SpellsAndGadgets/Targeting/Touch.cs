using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.SpellsAndGadgets;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Standard.SpellsAndGadgets.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.SpellsAndGadgets.Targeting
{
    public class Touch : ITargeting
    {
        public Guid TypeId => new Guid("144cc142-2e64-476f-b3a6-de57cc3abd05");

        public string TypeName => nameof(Touch);

        public bool HasShape => true;

        public bool IsContinuous => false;

        public bool IsParentedToSource => false;

        public bool IsServerSideOnly => true;

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Touch.prefab";

        public void SetBehaviourVariables(GameObject gameObject, SpellOrGadgetItemBase spellOrGadget, IFighter sourceFighter, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SogTouchBehaviour>();
            spellScript.SpellOrGadget = spellOrGadget;
            spellScript.SourceFighter = sourceFighter;
            spellScript.StartPosition = startPosition;
            spellScript.ForwardDirection = forwardDirection;
        }

    }
}
