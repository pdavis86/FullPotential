using FullPotential.Api.Data;
using FullPotential.Models;

using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerFighter : ISaveable
    {
        PlayerData GetPlayerData();

        void SpawnLootChest(Vector3 position);
    }
}
