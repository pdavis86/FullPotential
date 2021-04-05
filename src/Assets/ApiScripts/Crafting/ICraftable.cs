namespace Assets.ApiScripts.Crafting
{
    public interface ICraftable
    {
        public enum CraftingCategory
        {
            Weapon,
            Armor,
            Accessory,
            Spell
        }

        /// <summary>
        /// The category of this craftable
        /// </summary>
        CraftingCategory Category { get; }

        /// <summary>
        /// The name used to identify this craftable type
        /// </summary>
        string TypeName { get; }

        //todo: need to register a prefab when registering
        //todo: requirements e.g. strength, speed, accuracy, 6 scrap or less
    }
}
