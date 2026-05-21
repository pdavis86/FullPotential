using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;

using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IPlayerFighter : ISaveable
    {
        PlayerData GetPlayerData();

        void SpawnLootChest(Vector3 position);
    }
}
