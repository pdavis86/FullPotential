using System;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Standard.Loot
{
    public class Scrap : ILoot
    {
        public Guid TypeId => new Guid("9c267eb2-9bcf-4fe9-bfc9-6aea94b06e82");

        public Guid? ResourceTypeId => ResourceTypeIds.Energy;
    }
}