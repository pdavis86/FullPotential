using System;

using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Ammo;

namespace FullPotential.Standard.Weapons
{
    public class Gun : IWeaponType
    {
        public const string TypeIdString = "9b5a211a-07d2-4e5c-b8b8-639dbfb807e9";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public bool IsDefensive => false;

        public string AmmunitionTypeIdString => Bullet.TypeIdString;

        public bool AllowAutomatic => true;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
