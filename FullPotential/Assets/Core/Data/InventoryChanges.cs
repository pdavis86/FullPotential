﻿// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Assets.Core.Data
{
    [System.Serializable]
    public class InventoryChanges : Inventory
    {
        public string[] IdsToRemove;
    }
}