using System;
using FullPotential.Api.Gameplay.Player;

namespace FullPotential.Api.Data
{
    [Serializable]
    public class PlayerData
    {
        public string Username;
        public PlayerSettings Settings;
        public SerializableKeyValuePair<string, int>[] Resources;
        public InventoryData Inventory;

        [NonSerialized] public bool InventoryLoadedSuccessfully;
    }
}
