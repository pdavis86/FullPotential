using UnityEngine;

namespace FullPotential.Assets.Behaviours.SpellBehaviours
{
    public interface ISpellBehaviour
    {
        void ApplySpellEffects(GameObject target, Vector3? position);
    }
}
