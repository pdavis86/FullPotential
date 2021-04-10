namespace Assets.ApiScripts.Crafting
{
    public interface IGearAccessory : IGear
    {
        public enum AccessorySlot
        {
            Ring = IGear.GearSlot.Ring,
            Belt = IGear.GearSlot.Belt,
            Amulet = IGear.GearSlot.Amulet
        }

        /// <summary>
        /// Which accessory slot this item can occupy
        /// </summary>
        AccessorySlot InventorySlot { get; }
    }
}
