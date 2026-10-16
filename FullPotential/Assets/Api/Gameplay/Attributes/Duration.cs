using System;

using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Gameplay.Attributes
{
    public class Duration : IAttribute
    {
        public const string TypeIdString = "0f2e5b87-76ba-42db-9e3e-a56e5a44ef1d";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;
    }
}
