using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Sword : IGearWeapon
    {
        public string TypeName => "Sword";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
