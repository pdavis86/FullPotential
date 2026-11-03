using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Efficiency : IAttribute
    {
        public const string TypeIdString = "fbfbc4c7-4f3a-4ac4-8490-7b3018b14b5c";

        public static readonly AttributeTypeId Id = AttributeTypeId.FromString(TypeIdString);

        public Guid TypeId { get; } = new Guid(TypeIdString);
    }
}
