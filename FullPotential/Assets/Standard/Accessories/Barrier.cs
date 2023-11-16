using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Barrier : IAccessory
    {
        public const string TypeIdString = "17a6e875-cccd-46f0-b525-fe15cfdd8096";

        public Guid TypeId => new Guid(TypeIdString);

        public string TypeName => nameof(Barrier);
    }
}
