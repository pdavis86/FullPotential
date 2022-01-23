using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Spells
{
    public interface ISpellBehaviour
    {
        void ApplySpellEffects(GameObject target, Vector3? position);
    }
}
