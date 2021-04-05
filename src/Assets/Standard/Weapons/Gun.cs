using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Gun : ICraftableWeapon
    {
        public string TypeName => "Gun";

        public ICraftable.CraftingCategory Category => ICraftable.CraftingCategory.Weapon;

        public ICraftableWeapon.WeaponCategory SubCategory => ICraftableWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => true;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
