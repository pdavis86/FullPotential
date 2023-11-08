using System;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Utilities.Data;

namespace FullPotential.Api.Gameplay.Player
{
    [Serializable]
    public class PlayerData
    {
        public string Username;
        public PlayerSettings Settings;
        public KeyValuePair<string, int>[] Resources;
        public InventoryData Inventory;

        [NonSerialized] public bool InventoryLoadedSuccessfully;
    }
}
