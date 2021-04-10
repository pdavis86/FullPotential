using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Shield : IGearWeapon
    {
        public string TypeName => "Shield";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Defensive;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
