using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Bow : IGearWeapon
    {
        public string TypeName => "Bow";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
