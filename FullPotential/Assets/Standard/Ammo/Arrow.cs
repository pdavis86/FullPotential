using System;

using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Ammo
{
    public class Arrow : IAmmunitionType
    {
        private const string TypeIdString = "d84c2b09-f6b2-48cf-a781-c1025cdc56be";

        public static AmmunitionTypeId Id = AmmunitionTypeId.FromString(TypeIdString);

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public int MaxStackSize => 50;

        public int MinDropCount => 20;

        public int MaxDropCount => 40;
    }
}
