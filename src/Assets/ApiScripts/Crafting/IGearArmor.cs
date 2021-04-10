namespace Assets.ApiScripts.Crafting
{
    public interface IGearArmor : IGear
    {
        public enum ArmorSlot
        {
            Helm = IGear.GearSlot.Helm,
            Chest = IGear.GearSlot.Chest,
            Legs = IGear.GearSlot.Legs,
            Feet = IGear.GearSlot.Feet,
            Barrier = IGear.GearSlot.Barrier
        }

        /// <summary>
        /// Which armor slot this item can occupy
        /// </summary>
        ArmorSlot InventorySlot { get; }
    }
}
