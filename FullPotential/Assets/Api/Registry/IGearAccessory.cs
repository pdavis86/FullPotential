namespace FullPotential.Assets.Api.Registry
{
    public interface IGearAccessory : IGear
    {
        public enum AccessorySlot
        {
            Ring = GearSlot.Ring,
            Belt = GearSlot.Belt,
            Amulet = GearSlot.Amulet
        }

        /// <summary>
        /// Which accessory slot this item can occupy
        /// </summary>
        AccessorySlot InventorySlot { get; }
    }
}
