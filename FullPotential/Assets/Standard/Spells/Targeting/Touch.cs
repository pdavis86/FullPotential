using System;
using FullPotential.Api.Registry.Spells;
using FullPotential.Standard.Spells.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.Spells.Targeting
{
    public class Touch : ISpellTargeting
    {
        public Guid TypeId => new Guid("144cc142-2e64-476f-b3a6-de57cc3abd05");

        public string TypeName => nameof(Touch);

        public bool HasShape => true;

        public bool IsContinuous => false;

        public bool IsParentedToCaster => false;

        public bool IsServerSideOnly => true;

        public string PrefabAddress => "Standard/Prefabs/Spells/SpellTouch.prefab";

        public string IdlePrefabAddress => "Standard/Prefabs/Spells/SpellInHand.prefab";

        public void SetBehaviourVariables(GameObject gameObject, Spell activeSpell, Vector3 startPosition, Vector3 targetDirection, ulong casterClientId, bool isLeftHand = false)
        {
            var spellScript = gameObject.GetComponent<SpellTouchBehaviour>();
            spellScript.SpellId = activeSpell.Id;
            spellScript.StartPosition = startPosition;
            spellScript.SpellDirection = targetDirection;
            spellScript.PlayerClientId = casterClientId;
        }

    }
}
