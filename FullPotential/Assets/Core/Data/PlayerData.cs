﻿// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

using System;

namespace FullPotential.Core.Data
{
    [Serializable]
    public class PlayerData
    {
        public string Username;
        public PlayerOptions Options;
        public Inventory Inventory;

        [NonSerialized] public bool InventoryLoadedSuccessfully;
        [NonSerialized] public bool IsDirty;
    }
}
