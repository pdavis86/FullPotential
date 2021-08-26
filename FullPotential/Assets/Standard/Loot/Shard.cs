using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Loot
{
    public class Shard : ILoot
    {
        public Guid TypeId => new Guid("ffa1717c-2bc8-45e1-86b4-abbd148289fa");

        public string TypeName => nameof(Shard);

        public ILoot.LootCategory Category => ILoot.LootCategory.Magic;
    }
}
