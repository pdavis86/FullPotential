using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Strength : IAttribute
    {
        public const string TypeIdString = "12710b77-84f8-42ce-b9f0-28939d3cce54";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;
    }
}
