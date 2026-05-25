using System;

using FullPotential.Api.Data.Models;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Obsolete
{
    [Serializable]
    public class PlayerDataOld
    {
        public string Username;
        public CharacterSettings Settings;
        public SerializableKeyValuePair<string, int>[] Resources;
        public InventoryData Inventory;

        [NonSerialized] public bool InventoryLoadedSuccessfully;
    }
}
