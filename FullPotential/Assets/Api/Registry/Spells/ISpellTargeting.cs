using UnityEngine;

namespace FullPotential.Api.Registry.Spells
{
    public interface ISpellTargeting : IRegisterable, IHasPrefab, IHasIdlePrefab
    {
        bool HasShape { get; }

        bool IsContinuous { get; }

        bool IsParentedToCaster { get; }

        void SetBehaviourVariables(
            GameObject gameObject,
            Spell activeSpell, 
            Vector3 startPosition, 
            Vector3 targetDirection,
            ulong senderClientId,
            bool isLeftHand = false);
    }
}
