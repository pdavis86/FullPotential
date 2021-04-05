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

        CraftingCategory Category { get; }
        string TypeName { get; }

        //todo: need to register a prefab when registering
        //todo: requirements e.g. strength, speed, accuracy, 6 scrap or less
    }
}
