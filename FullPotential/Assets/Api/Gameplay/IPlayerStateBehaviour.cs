using FullPotential.Api.Combat;
using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerStateBehaviour : IDamageable, IDefensible
    {
        IPlayerInventory Inventory { get; }

        GameObject PlayerCameraGameObject { get; }

        ulong OwnerClientId { get; }

        void SpawnLootChest(Vector3 position);
    }
}
