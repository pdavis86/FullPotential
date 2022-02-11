using FullPotential.Api.Combat;
using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerStateBehaviour : IDamageable, IDefensible
    {
        ulong OwnerClientId { get; }

        Transform Transform { get; }

        GameObject GameObject { get; }

        GameObject CameraGameObject { get; }

        IPlayerInventory Inventory { get; }

        void SpawnLootChest(Vector3 position);
    }
}
