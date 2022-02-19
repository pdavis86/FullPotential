using FullPotential.Api.Gameplay;
using UnityEngine;

namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    public interface IShape : IRegisterable, IHasPrefab
    {
        void SpawnGameObject(
            SpellOrGadgetItemBase spellOrGadget, 
            IPlayerStateBehaviour sourceStateBehaviour,
            Vector3 startPosition, 
            Quaternion startRotation);
    }
}
