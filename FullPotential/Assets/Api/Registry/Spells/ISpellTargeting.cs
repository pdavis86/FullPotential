using FullPotential.Api.Gameplay;
using UnityEngine;

namespace FullPotential.Api.Registry.Spells
{
    public interface ISpellTargeting : IRegisterable, IHasPrefab, IHasIdlePrefab
    {
        bool HasShape { get; }

        bool IsContinuous { get; }

        bool IsParentedToCaster { get; }

        bool IsServerSideOnly { get; }

        void SetBehaviourVariables(
            GameObject gameObject,
            Spell spell,
            IPlayerStateBehaviour sourceStateBehaviour,
            Vector3 startPosition, 
            Vector3 forwardDirection,
            bool isLeftHand = false);
    }
}
