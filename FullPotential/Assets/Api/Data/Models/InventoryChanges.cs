using System;

using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Data.Models
{
    // todo: kill InventoryChanges
    [Serializable]
    public class InventoryChanges : InventoryData
    {
        public string[] IdsToRemove;
    }
}
