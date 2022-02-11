using FullPotential.Api.Gameplay;
using UnityEngine;

namespace FullPotential.Api.Registry.Spells
{
    public interface ISpellShape : IRegisterable, IHasPrefab
    {
        void SpawnGameObject(
            Spell spell, 
            IPlayerStateBehaviour sourceStateBehaviour,
            Vector3 startPosition, 
            Quaternion startRotation);
    }
}
