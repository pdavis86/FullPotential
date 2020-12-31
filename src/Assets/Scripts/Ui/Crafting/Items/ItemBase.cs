using System.Collections.Generic;

namespace Assets.Scripts.Ui.Crafting.Items
{
    public abstract class ItemBase
    {
        public const string Weapon = "Weapon";
        public const string Armor = "Armor";
        public const string Accessory = "Accessory";
        public const string Spell = "Spell";

        public const string OneHanded = "One-handed";
        public const string TwoHanded = "Two-handed";

        public Attributes Attributes { get; set; }
        public int Damage { get; set; }
        public List<string> Effects { get; set; }
    }
}
