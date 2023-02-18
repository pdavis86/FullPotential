using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Crafting;
using UnityEngine;

namespace FullPotential.Api.Registry.Consumers
{
    public interface ITargeting : IRegisterable, IHasPrefab
    {
        bool HasShape { get; }

        bool IsContinuous { get; }

        bool IsParentedToSource { get; }

        bool IsServerSideOnly { get; }

        void SetBehaviourVariables(
            GameObject gameObject,
            Consumer consumer,
            IFighter sourceFighter,
            Vector3 startPosition, 
            Vector3 forwardDirection,
            bool isLeftHand = false);
    }
}
