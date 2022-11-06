using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Crafting;
using UnityEngine;

namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    public interface IShape : IRegisterable, IHasPrefab
    {
        void SpawnGameObject(
            SpellOrGadgetItemBase spellOrGadget,
            IFighter sourceStateBehaviour,
            Vector3 startPosition, 
            Quaternion startRotation);
    }
}
