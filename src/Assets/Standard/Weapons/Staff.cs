using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Staff : IGearWeapon
    {
        public string TypeName => "Staff";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
