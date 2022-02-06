using FullPotential.Api.Data;

namespace FullPotential.Core.Data
{
    [System.Serializable]
    public class InventoryChanges : InventoryData
    {
        public string[] IdsToRemove;
    }
}
