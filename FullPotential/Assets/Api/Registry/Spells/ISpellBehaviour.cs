using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Registry.Spells
{
    public interface ISpellBehaviour
    {
        //void StartCasting();

        void StopCasting();
        
        void ApplySpellEffects(GameObject target, Vector3? position);
    }
}
