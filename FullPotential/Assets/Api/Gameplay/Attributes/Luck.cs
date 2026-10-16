using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Luck : IAttribute
    {
        public const string TypeIdString = "14bbd099-d586-48f5-a911-38b4a455d0fb";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;
    }
}
