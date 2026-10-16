using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Speed : IAttribute
    {
        public const string TypeIdString = "c82be4ee-22ee-4b7a-b551-47e40647b4af";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;
    }
}
