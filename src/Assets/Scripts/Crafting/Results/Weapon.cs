using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Assets.Scripts.Crafting.Results
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

        public static readonly List<string> WeaponOptions = new List<string>
        {
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
