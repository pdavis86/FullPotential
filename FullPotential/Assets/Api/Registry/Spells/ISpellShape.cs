using UnityEngine;

namespace FullPotential.Api.Registry.Spells
{
    public interface ISpellShape : IRegisterable
    {
        void SpawnGameObject(Spell activeSpell, Vector3 startPosition, Quaternion rotation, ulong senderClientId);
    }
}
