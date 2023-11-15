using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Helm : IArmor
    {
        public const string TypeIdString = "bd6f655b-6fed-42e5-8797-e9cb3f675696";

        public Guid TypeId => new Guid(TypeIdString);

        public string TypeName => nameof(Helm);
    }
}
