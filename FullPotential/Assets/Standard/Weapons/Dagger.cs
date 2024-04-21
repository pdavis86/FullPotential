using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Dagger : IWeaponType
    {
        public const string TypeIdString = "6eabecda-d308-48b2-b7d3-93c7800df8c2";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool IsDefensive => false;

        public string AmmunitionTypeIdString => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => false;

        public bool EnforceTwoHanded => false;
    }
}
