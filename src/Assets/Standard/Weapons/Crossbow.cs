using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Crossbow : IGearWeapon
    {
        public string TypeName => "Crossbow";

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
