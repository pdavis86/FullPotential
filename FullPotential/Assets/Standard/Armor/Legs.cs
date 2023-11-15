using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Legs : IArmor
    {
        public const string TypeIdString = "b4ec0616-8a9c-4052-a318-482a305bb263";

        public Guid TypeId => new Guid(TypeIdString);

        public string TypeName => nameof(Legs);
    }
}
