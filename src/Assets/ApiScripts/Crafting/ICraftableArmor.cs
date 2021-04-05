namespace Assets.ApiScripts.Crafting
{
    public interface ICraftableArmor : ICraftable
    {
        public enum ArmorCategory
        {
            Helm,
            Chest,
            Legs,
            Feet,
            Barrier
        }

        /// <summary>
        /// The type of Armor
        /// </summary>
        ArmorCategory SubCategory { get; }
    }
}
