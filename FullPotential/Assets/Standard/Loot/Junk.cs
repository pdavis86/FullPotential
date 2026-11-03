using System;

using FullPotential.Api.Registry;

namespace FullPotential.Standard.Loot
{
    public class Junk : ILootType
    {
        public Guid TypeId { get; } = new Guid("68452756-7f9f-404e-bce5-1074355ae122");
    }
}
