namespace Assets.ApiScripts.Crafting
{
    public interface IGearLoot : IGear
    {
        public enum LootCategory
        {
            Technology,
            Magic
        }

        /// <summary>
        /// The category of loot
        /// </summary>
        LootCategory Category { get; }
    }
}
