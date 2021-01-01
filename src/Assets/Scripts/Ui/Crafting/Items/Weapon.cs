using System.Collections.Generic;

namespace Assets.Scripts.Ui.Crafting.Items
{
    public class Weapon : ItemBase
    {
        public const string Dagger = "Dagger";
        public const string Axe = "Axe";
        public const string Sword = "Sword";
        public const string Hammer = "Hammer";
        public const string Spear = "Spear";
        public const string Bow = "Bow";
        public const string Crossbow = "Crossbow";
        public const string Gun = "Gun";
        public const string Shield = "Shield";

        public const string OneHanded = "One-handed";
        public const string TwoHanded = "Two-handed";

        public static List<string> WeaponOptions = new List<string> {
            Dagger,
            Axe,
            Sword,
            Hammer,
            Spear,
            Bow,
            Crossbow,
            Gun,
            Shield
        };

        public bool IsTwoHanded { get; set; }

    }
}
