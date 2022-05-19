using FullPotential.Api.Gameplay.Combat;
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
