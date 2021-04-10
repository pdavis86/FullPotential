using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Hammer : IGearWeapon
    {
        public string TypeName => "Hammer";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
