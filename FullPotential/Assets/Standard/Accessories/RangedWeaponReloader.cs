using System;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class RangedWeaponReloader : IAccessory
    {
        public const string TypeIdString = "575ed70f-f5de-4ffa-93fb-a6c1cc404f30";

        public Guid TypeId => new Guid(TypeIdString);

        public string TypeName => nameof(RangedWeaponReloader);

        public AccessoryLocation Location => AccessoryLocation.Other;
    }
}
