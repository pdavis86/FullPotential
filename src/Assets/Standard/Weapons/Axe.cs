using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Axe : IGearWeapon
    {
        public string TypeName => "Axe";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
