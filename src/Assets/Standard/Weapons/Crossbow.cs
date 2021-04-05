using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Weapons
{
    public class Crossbow : ICraftableWeapon
    {
        public string TypeName => "Crossbow";

        public ICraftable.CraftingCategory Category => ICraftable.CraftingCategory.Weapon;

        public ICraftableWeapon.WeaponCategory SubCategory => ICraftableWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
