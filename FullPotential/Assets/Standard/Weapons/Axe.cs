using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Axe : IWeapon
    {
        public const string TypeIdString = "0bef1fe6-4b04-4700-bd51-6ff82a10703b";

        public Guid TypeId => new Guid(TypeIdString);

        public bool IsDefensive => false;

        public Guid? AmmunitionTypeId => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
