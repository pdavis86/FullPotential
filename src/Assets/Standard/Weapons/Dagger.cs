using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Dagger : IGearWeapon
    {
        public string TypeName => "Dagger";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => false;

        public bool EnforceTwoHanded => false;
    }
}
