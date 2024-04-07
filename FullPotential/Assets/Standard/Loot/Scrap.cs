using System;
using FullPotential.Api.Registry;

namespace FullPotential.Standard.Loot
{
    public class Scrap : ILoot
    {
        private static readonly Guid Id = new Guid("9c267eb2-9bcf-4fe9-bfc9-6aea94b06e82");

        public Guid TypeId => Id;
    }
}