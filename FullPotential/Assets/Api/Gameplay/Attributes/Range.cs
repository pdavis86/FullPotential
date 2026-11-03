using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Range : IAttribute
    {
        public const string TypeIdString = "371612f4-ce57-4cf8-a769-e5234b35aba6";

        public static readonly AttributeTypeId Id = AttributeTypeId.FromString(TypeIdString);

        public Guid TypeId { get; } = new Guid(TypeIdString);
    }
}
