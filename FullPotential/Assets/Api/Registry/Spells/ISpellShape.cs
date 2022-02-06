using UnityEngine;

namespace FullPotential.Api.Registry.Spells
{
    public interface ISpellShape : IRegisterable, IHasPrefab
    {
        void SpawnGameObject(Spell activeSpell, Vector3 startPosition, Quaternion rotation, ulong senderClientId);
    }
}
