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

        public string PrefabAddress => null;

        public string IdlePrefabAddress => null;

        public void SetBehaviourVariables(GameObject gameObject, Spell activeSpell, Vector3 startPosition, Vector3 targetDirection, ulong senderClientId, bool isLeftHand = false, Transform parentTransform = null)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new SpellTouchBehaviour(activeSpell, startPosition, targetDirection, senderClientId);
        }

    }
}
