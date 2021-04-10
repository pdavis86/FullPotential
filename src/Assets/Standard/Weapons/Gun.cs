using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Weapons
{
    public class Gun : IGearWeapon
    {
        public Guid TypeId => new Guid("9b5a211a-07d2-4e5c-b8b8-639dbfb807e9");

        public string TypeName => nameof(Gun);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => true;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
