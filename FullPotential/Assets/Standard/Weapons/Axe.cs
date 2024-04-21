using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Axe : IWeaponType
    {
        public const string TypeIdString = "0bef1fe6-4b04-4700-bd51-6ff82a10703b";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool IsDefensive => false;

        public string AmmunitionTypeIdString => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
