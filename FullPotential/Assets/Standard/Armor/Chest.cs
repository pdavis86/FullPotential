using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Chest : IArmor
    {
        public const string TypeIdString = "2419fcac-217e-48d8-9770-76c5ff27c9f8";

        public Guid TypeId => new Guid(TypeIdString);

        public string TypeName => nameof(Chest);
    }
}
