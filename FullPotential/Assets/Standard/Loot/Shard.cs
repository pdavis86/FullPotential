using System;
using FullPotential.Api.Registry;

namespace FullPotential.Standard.Loot
{
    public class Shard : ILootType
    {
        private static readonly Guid Id = new Guid("ffa1717c-2bc8-45e1-86b4-abbd148289fa");

        public Guid TypeId => Id;
    }
}
