using System;
using FullPotential.Api.Registry;

namespace FullPotential.Standard.Loot
{
    public class Junk : ILootType
    {
        private static readonly Guid Id = new Guid("68452756-7f9f-404e-bce5-1074355ae122");

        public Guid TypeId => Id;
    }
}
