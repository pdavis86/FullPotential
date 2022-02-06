using UnityEngine;

namespace FullPotential.Api.Registry.Spells
{
    public interface ISpellTargeting : IRegisterable
    {
        bool HasShape { get; }

        bool IsContinuous { get; }

        GameObject SpawnGameObject(
            Spell activeSpell, 
            Vector3 startPosition, 
            Vector3 targetDirection,
            ulong senderClientId,
            bool isLeftHand = false,
            Transform parentTransform = null);
    }
}
