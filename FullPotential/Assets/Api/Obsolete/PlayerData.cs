using System;

using FullPotential.Api.Data.Models;
using FullPotential.Api.Gameplay.Player;

namespace FullPotential.Api.Obsolete
{
    [Serializable]
    public class PlayerData
    {
        public string Username;
        public CharacterSettings Settings;
        public SerializableKeyValuePair<string, int>[] Resources;
        public InventoryData Inventory;

        [NonSerialized] public bool InventoryLoadedSuccessfully;
    }
}
