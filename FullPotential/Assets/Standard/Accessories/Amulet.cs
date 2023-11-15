using System;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Amulet : IAccessory
    {
        public const string TypeIdString = "ddeafb61-0163-4888-b355-16a37d3a33b5";

        public Guid TypeId => new Guid(TypeIdString);

        public string TypeName => nameof(Amulet);

        public AccessoryLocation Location => AccessoryLocation.Neck;
    }
}
