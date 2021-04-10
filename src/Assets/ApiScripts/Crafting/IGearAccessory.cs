namespace Assets.ApiScripts.Crafting
{
    public interface IGearAccessory : IGear
    {
        public enum AccessorySlots
        {
            Ring = IGear.InventorySlots.Ring,
            Belt = IGear.InventorySlots.Belt,
            Amulet = IGear.InventorySlots.Amulet
        }

        /// <summary>
        /// Which accessory slot this item can occupy
        /// </summary>
        AccessorySlots InventorySlot { get; }
    }
}
