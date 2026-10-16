using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Accuracy : IAttribute
    {
        public const string TypeIdString = "f8c338db-5160-4d37-a573-0e8137e8b3ff";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;
    }
}
