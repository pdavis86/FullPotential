using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Loot
{
    public class Scrap : IGearLoot
    {
        public string TypeName => "Scrap";

        public IGearLoot.LootCategory Category => IGearLoot.LootCategory.Technology;
    }
}