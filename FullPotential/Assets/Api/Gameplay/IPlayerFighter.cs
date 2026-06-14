using FullPotential.Api.Data;
using FullPotential.Models.Player;

using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerFighter : ISaveable
    {
        CharacterData GetCharacterData();

        void SpawnLootChest(Vector3 position);
    }
}
