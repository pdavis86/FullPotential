using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Shield : ICraftableWeapon
    {
        public string TypeName => "Shield";

        public ICraftable.CraftingCategory Category => ICraftable.CraftingCategory.Weapon;

        public ICraftableWeapon.WeaponCategory SubCategory => ICraftableWeapon.WeaponCategory.Defensive;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
