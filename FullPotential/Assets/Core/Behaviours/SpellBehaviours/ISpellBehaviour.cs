using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Core.Behaviours.SpellBehaviours
{
    public interface ISpellBehaviour
    {
        void ApplySpellEffects(GameObject target, Vector3? position);
    }
}
