using System;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.Spells;
using FullPotential.Standard.Spells.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.Spells.Targeting
{
    public class Projectile : ISpellTargeting
    {
        public Guid TypeId => new Guid("6e41729e-bb21-44f8-8fb9-b9ad48c0e680");

        public string TypeName => nameof(Projectile);

        public bool HasShape => true;

        public bool IsContinuous => false;

        public bool IsParentedToCaster => false;

        public bool IsServerSideOnly => false;

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellProjectile.prefab";

        public string IdlePrefabAddress => "Standard/Prefabs/Spells/SpellInHand.prefab";

        public void SetBehaviourVariables(GameObject gameObject, Spell spell, IPlayerStateBehaviour sourceStateBehaviour, Vector3 startPosition, Vector3 forwardDirection, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SpellProjectileBehaviour>();
            spellScript.Spell = spell;
            spellScript.SourceStateBehaviour = sourceStateBehaviour;
            spellScript.ForwardDirection = forwardDirection;
        }
        
    }
}
