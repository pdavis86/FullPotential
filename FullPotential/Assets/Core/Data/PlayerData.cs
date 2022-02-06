using System;
using FullPotential.Api.Data;

namespace FullPotential.Core.Data
{
    [Serializable]
    public class PlayerData
    {
        public string Username;
        public PlayerSettings Settings;
        public InventoryData Inventory;

        [NonSerialized] public bool InventoryLoadedSuccessfully;
        [NonSerialized] public bool IsDirty;
    }
}
