using System.Collections.Generic;

namespace Assets.Scripts.Crafting.Results
{
    public class Armor : ItemBase
    {
        public const string Helm = "Helm";
        public const string Chest = "Chest";
        public const string Legs = "Legs";
        public const string Feet = "Feet";
        public const string Gloves = "Gloves";
        public const string Barrier = "Barrier";

        public static List<string> ArmorOptions = new List<string> {
            Helm,
            Chest,
            Legs,
            Feet,
            Gloves,
            Barrier
        };

    }
}
