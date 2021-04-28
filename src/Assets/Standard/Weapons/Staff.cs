using Assets.ApiScripts.Registry;
using System;

namespace Assets.Standard.Weapons
{
    public class Staff : IGearWeapon
    {
        public Guid TypeId => new Guid("081f7708-9909-424f-bdd1-f39f487c018a");

        public string TypeName => nameof(Staff);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
