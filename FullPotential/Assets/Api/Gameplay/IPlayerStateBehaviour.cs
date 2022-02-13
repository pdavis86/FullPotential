using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Spells;
using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerStateBehaviour : IDamageable, IDefensible
    {
        ulong OwnerClientId { get; }

        Transform Transform { get; }

        GameObject GameObject { get; }

        GameObject CameraGameObject { get; }

        void SpawnLootChest(Vector3 position);

        bool SpendMana(Spell activeSpell, bool slowDrain = false);
    }
}
