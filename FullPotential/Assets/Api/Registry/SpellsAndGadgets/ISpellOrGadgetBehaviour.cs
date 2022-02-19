using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    public interface ISpellOrGadgetBehaviour
    {
        void Stop();
        
        void ApplyEffects(GameObject target, Vector3? position);
    }
}
