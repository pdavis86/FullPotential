using System;

using FullPotential.Api.Registry.Weapons;


namespace FullPotential.Standard.Ammo
{
    public class Bullet : IAmmunitionType
    {
        private const string TypeIdString = "130cc39d-e7db-4b8b-bb09-53a5d68fa05d";

        public static AmmunitionTypeId Id = AmmunitionTypeId.FromString(TypeIdString);

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public int MaxStackSize => 500;

        public int MinDropCount => 100;

        public int MaxDropCount => 400;
    }
}
