﻿using System;
using FullPotential.Api.Gameplay.Data;

namespace FullPotential.Core.Gameplay.Data
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