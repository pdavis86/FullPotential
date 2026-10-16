using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Range : IAttribute
    {
        public const string TypeIdString = "371612f4-ce57-4cf8-a769-e5234b35aba6";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;
    }
}
