using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Consumers;
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

        public void SetBehaviourVariables(GameObject gameObject, Consumer consumer, IFighter sourceFighter, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SogBeamBehaviour>();
            spellScript.Consumer = consumer;
            spellScript.SourceFighter = sourceFighter;
            spellScript.IsLeftHand = isLeftHand;
        }

    }
}
