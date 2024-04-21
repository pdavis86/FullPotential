using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Ammo;

namespace FullPotential.Standard.Weapons
{
    public class Bow : IWeaponType
    {
        public const string TypeIdString = "47d23976-45ad-4360-b603-7ea4ed29846b";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool IsDefensive => false;

        public string AmmunitionTypeIdString => Arrow.TypeIdString;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
