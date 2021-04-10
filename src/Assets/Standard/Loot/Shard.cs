using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Loot
{
    public class Shard : IGearLoot
    {
        public string TypeName => "Shard";

        public IGearLoot.LootCategory Category => IGearLoot.LootCategory.Magic;
    }
}
