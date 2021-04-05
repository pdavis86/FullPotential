using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Bow : ICraftableWeapon
    {
        public string TypeName => "Bow";

        public ICraftable.CraftingCategory Category => ICraftable.CraftingCategory.Weapon;

        public ICraftableWeapon.WeaponCategory SubCategory => ICraftableWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
