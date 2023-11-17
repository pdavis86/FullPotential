using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Shield : IWeapon
    {
        public const string TypeIdString = "2b0d5e47-77b0-4311-98ee-0e41827f5fc4";

        public Guid TypeId => new Guid(TypeIdString);

        public bool IsDefensive => true;

        public Guid? AmmunitionTypeId => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => false;

        public bool EnforceTwoHanded => false;
    }
}
