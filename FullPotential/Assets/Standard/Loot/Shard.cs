using System;
using FullPotential.Api.Registry;

namespace FullPotential.Standard.Loot
{
    public class Shard : ILoot
    {
        public Guid TypeId => new Guid("ffa1717c-2bc8-45e1-86b4-abbd148289fa");
    }
}
