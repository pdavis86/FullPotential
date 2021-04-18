using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Loot
{
    public class Scrap : ILoot
    {
        public Guid TypeId => new Guid("9c267eb2-9bcf-4fe9-bfc9-6aea94b06e82");

        public string TypeName => nameof(Scrap);

        public ILoot.LootCategory Category => ILoot.LootCategory.Technology;
    }
}