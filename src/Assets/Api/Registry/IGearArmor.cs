namespace Assets.ApiScripts.Registry
{
    public interface IGearArmor : IGear
    {
        public enum ArmorSlot
        {
            Helm = GearSlot.Helm,
            Chest = GearSlot.Chest,
            Legs = GearSlot.Legs,
            Feet = GearSlot.Feet,
            Barrier = GearSlot.Barrier
        }

        /// <summary>
        /// Which armor slot this item can occupy
        /// </summary>
        ArmorSlot InventorySlot { get; }
    }
}
