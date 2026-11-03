using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Speed : IAttribute
    {
        public const string TypeIdString = "c82be4ee-22ee-4b7a-b551-47e40647b4af";

        public static readonly AttributeTypeId Id = AttributeTypeId.FromString(TypeIdString);

        public Guid TypeId { get; } = new Guid(TypeIdString);
    }
}
