using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;

namespace FullPotential.Standard.Loot
{
    public class Junk : ILoot
    {
        public Guid TypeId => new Guid("68452756-7f9f-404e-bce5-1074355ae122");

        public string TypeName => nameof(Shard);

        public ResourceType? ResourceConsumptionType => null;
    }
}
