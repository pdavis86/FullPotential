using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Hammer : ICraftableWeapon
    {
        public string TypeName => "Hammer";

        public ICraftable.CraftingCategory Category => ICraftable.CraftingCategory.Weapon;

        public ICraftableWeapon.WeaponCategory SubCategory => ICraftableWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;
    }
}
