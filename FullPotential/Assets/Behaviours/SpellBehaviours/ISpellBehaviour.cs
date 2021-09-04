using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Assets.Behaviours.SpellBehaviours
{
    public interface ISpellBehaviour
    {
        void ApplySpellEffects(GameObject target, Vector3? position);
    }
}
