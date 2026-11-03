using System;

using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Sword : IWeaponType
    {
        public const string TypeIdString = "b1ff5c3c-a306-4a2a-9fef-24320e05e74f";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public bool IsDefensive => false;

        public string AmmunitionTypeIdString => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
