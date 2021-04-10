using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Gun : IGearWeapon
    {
        public string TypeName => "Gun";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => true;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
