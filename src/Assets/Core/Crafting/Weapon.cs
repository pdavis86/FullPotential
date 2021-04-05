// ReSharper disable UnusedAutoPropertyAccessor.Global

using Assets.ApiScripts.Crafting;

namespace Assets.Core.Crafting
{
    [System.Serializable]
    public class Weapon : GearBase, ICraftableWeapon
    {
        public string TypeName { get; set; }

        public ICraftable.CraftingCategory Category { get; set; }

        public ICraftableWeapon.WeaponCategory SubCategory { get; set; }

        public bool AllowAutomatic { get; set; }

        public bool AllowTwoHanded { get; set; }

        public bool EnforceTwoHanded { get; set; }


        public bool IsTwoHanded;

    }
}
