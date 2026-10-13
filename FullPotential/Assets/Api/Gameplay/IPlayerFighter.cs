using FullPotential.Api.Data;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Models.LivingEntity;

using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerFighter : IFighter, ISaveable
    {
        IPlayerInventory PlayerInventory { get; }

        GameObject InFrontOfPlayerGameObject { get; }

        CharacterData GetCharacterData();

        void SpawnLootChest(Vector3 position);

        bool IsLocalClient();
    }
}
