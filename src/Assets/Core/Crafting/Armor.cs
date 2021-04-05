using Assets.ApiScripts.Crafting;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Assets.Core.Crafting
{
    [System.Serializable]
    public class Armor : GearBase, ICraftableArmor
    {
        public string TypeName { get; set; }

        public ICraftable.CraftingCategory Category { get; set; }

        public ICraftableArmor.ArmorCategory SubCategory { get; set; }




        //todo: everything below here should be "registered" rather than hard-coded

        public const string Helm = "Helm";
        public const string Chest = "Chest";
        public const string Legs = "Legs";
        public const string Feet = "Feet";
        public const string Barrier = "Barrier";

        public static readonly List<string> ArmorOptions = new List<string>
        {
            Helm,
            Chest,
            Legs,
            Feet,
            Barrier
        };

    }
}
