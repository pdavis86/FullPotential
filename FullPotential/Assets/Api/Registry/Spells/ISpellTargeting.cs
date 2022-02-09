using UnityEngine;

namespace FullPotential.Api.Registry.Spells
{
    public interface ISpellTargeting : IRegisterable, IHasPrefab, IHasIdlePrefab
    {
        bool HasShape { get; }

        bool IsContinuous { get; }

        bool IsParentedToCaster { get; }

        bool IsServerSideOnly { get; }

        //todo: don't pass IDs, pass the actual objects instead
        void SetBehaviourVariables(
            GameObject gameObject,
            Spell activeSpell, 
            Vector3 startPosition, 
            Vector3 targetDirection,
            ulong casterClientId,
            bool isLeftHand = false);
    }
}
