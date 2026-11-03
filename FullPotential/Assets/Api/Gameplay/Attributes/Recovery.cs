using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Recovery : IAttribute
    {
        public const string TypeIdString = "a8e96df0-fe6a-4371-bf7e-2c84a0d830bc";

        public static readonly AttributeTypeId Id = AttributeTypeId.FromString(TypeIdString);

        public Guid TypeId { get; } = new Guid(TypeIdString);
    }
}
