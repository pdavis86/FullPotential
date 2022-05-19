using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Standard.SpellsAndGadgets.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.SpellsAndGadgets.Targeting
{
    public class Beam : ITargeting
    {
        public Guid TypeId => new Guid("3c528d09-bd4f-4211-af17-d8721975fef1");

        public string TypeName => nameof(Beam);

        public bool HasShape => false;

        public bool IsContinuous => true;

        public bool IsParentedToSource => true;

        public bool IsServerSideOnly => false;

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Beam.prefab";

        public void SetBehaviourVariables(GameObject gameObject, SpellOrGadgetItemBase spellOrGadget, IFighter sourceFighter, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SogBeamBehaviour>();
            spellScript.SpellOrGadget = spellOrGadget;
            spellScript.SourceFighter = sourceFighter;
            spellScript.IsLeftHand = isLeftHand;
        }

    }
}
