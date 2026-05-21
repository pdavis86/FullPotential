using System;

namespace FullPotential.Api.Data.Models
{
    [Serializable]
    public class InventoryChanges : InventoryData
    {
        public string[] IdsToRemove;
    }
}
