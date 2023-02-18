using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Consumers;
using FullPotential.Standard.SpellsAndGadgets.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.SpellsAndGadgets.Targeting
{
    public class Projectile : ITargeting
    {
        public Guid TypeId => new Guid("6e41729e-bb21-44f8-8fb9-b9ad48c0e680");

        public string TypeName => nameof(Projectile);

        public bool HasShape => true;

        public bool IsContinuous => false;

        public bool IsParentedToSource => false;

        public bool IsServerSideOnly => false;

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Projectile.prefab";

        public void SetBehaviourVariables(GameObject gameObject, Consumer consumer, IFighter sourceFighter, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SogProjectileBehaviour>();
            spellScript.Consumer = consumer;
            spellScript.SourceFighter = sourceFighter;
            spellScript.ForwardDirection = forwardDirection;
        }
        
    }
}
