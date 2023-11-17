using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Staff : IWeapon
    {
        public const string TypeIdString = "081f7708-9909-424f-bdd1-f39f487c018a";

        public Guid TypeId => new Guid(TypeIdString);

        public bool IsDefensive => false;

        public Guid? AmmunitionTypeId => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
