using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Staff : IWeaponType
    {
        public const string TypeIdString = "081f7708-9909-424f-bdd1-f39f487c018a";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool IsDefensive => false;

        public string AmmunitionTypeIdString => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
