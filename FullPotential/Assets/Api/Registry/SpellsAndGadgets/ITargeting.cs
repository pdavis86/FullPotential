using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Crafting;
using UnityEngine;

namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    public interface ITargeting : IRegisterable, IHasPrefab
    {
        bool HasShape { get; }

        bool IsContinuous { get; }

        bool IsParentedToSource { get; }

        bool IsServerSideOnly { get; }

        void SetBehaviourVariables(
            GameObject gameObject,
            SpellOrGadgetItemBase spellOrGadget,
            IFighter sourceFighter,
            Vector3 startPosition, 
            Vector3 forwardDirection,
            bool isLeftHand = false);
    }
}
