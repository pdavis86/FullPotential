using Assets.ApiScripts.Registry;
using System;

namespace Assets.Standard.Weapons
{
    public class Sword : IGearWeapon
    {
        public Guid TypeId => new Guid("b1ff5c3c-a306-4a2a-9fef-24320e05e74f");

        public string TypeName => nameof(Sword);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
