using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Loot
{
    public class Shard : IGearLoot
    {
        public Guid TypeId => new Guid("ffa1717c-2bc8-45e1-86b4-abbd148289fa");

        public string TypeName => nameof(Shard);

        public IGearLoot.LootCategory Category => IGearLoot.LootCategory.Magic;
    }
}
