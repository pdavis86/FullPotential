using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Feet : IArmor
    {
        public const string TypeIdString = "645b2a9b-02df-4fb7-bea2-9b7a2a9c620b";

        public Guid TypeId => new Guid(TypeIdString);

        public string TypeName => nameof(Feet);
    }
}
