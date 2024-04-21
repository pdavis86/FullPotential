using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Shield : IWeaponType
    {
        public const string TypeIdString = "2b0d5e47-77b0-4311-98ee-0e41827f5fc4";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool IsDefensive => true;

        public string AmmunitionTypeIdString => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => false;

        public bool EnforceTwoHanded => false;
    }
}
