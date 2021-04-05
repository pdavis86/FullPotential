using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Dagger : ICraftableWeapon
    {
        public string TypeName => "Dagger";

        public ICraftable.CraftingCategory Category => ICraftable.CraftingCategory.Weapon;

        public ICraftableWeapon.WeaponCategory SubCategory => ICraftableWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => false;

        public bool EnforceTwoHanded => false;
    }
}
