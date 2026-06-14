using System;

using FullPotential.Models.Player;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Obsolete
{
    [Serializable]
    public class PlayerDataOld
    {
        public string Username;
        public CharacterSettingsOld Settings;
        public SerializableKeyValuePair<string, int>[] Resources;
        public InventoryDataOld Inventory;

        [NonSerialized] public bool InventoryLoadedSuccessfully;
    }
}
