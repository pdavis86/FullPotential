namespace Assets.ApiScripts.Crafting
{
    public interface IGearArmor : IGear
    {
        public enum ArmorSlots
        {
            Helm = IGear.InventorySlots.Helm,
            Chest = IGear.InventorySlots.Chest,
            Legs = IGear.InventorySlots.Legs,
            Feet = IGear.InventorySlots.Feet,
            Barrier = IGear.InventorySlots.Barrier
        }

        /// <summary>
        /// Which armor slot this item can occupy
        /// </summary>
        ArmorSlots InventorySlot { get; }
    }
}
