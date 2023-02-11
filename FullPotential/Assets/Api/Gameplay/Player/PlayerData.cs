using System;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Gameplay.Player
{
    [Serializable]
    public class PlayerData
    {
        public string Username;
        public PlayerSettings Settings;
        public ResourceType ResourceType;
        public InventoryData Inventory;

        [NonSerialized] public bool InventoryLoadedSuccessfully;
    }
}
