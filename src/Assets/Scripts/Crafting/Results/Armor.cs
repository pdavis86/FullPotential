using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Assets.Scripts.Crafting.Results
{
    [System.Serializable]
    public class Armor : GearBase
    {
        public const string Helm = "Helm";
        public const string Chest = "Chest";
        public const string Legs = "Legs";
        public const string Feet = "Feet";
        public const string Gloves = "Gloves";
        public const string Barrier = "Barrier";

        public static readonly List<string> ArmorOptions = new List<string>
        {
            Helm,
            Chest,
            Legs,
            Feet,
            Gloves,
            Barrier
        };

    }
}
