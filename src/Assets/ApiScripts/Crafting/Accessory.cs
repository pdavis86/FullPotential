using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Assets.Scripts.Crafting.Results
{
    [System.Serializable]
    public class Accessory : GearBase
    {
        public const string Gloves = "Gloves";
        public const string Amulet = "Amulet";
        public const string Ring = "Ring";
        public const string Belt = "Belt";

        public static readonly List<string> AccessoryOptions = new List<string>
        {
            Gloves,
            Amulet,
            Ring,
            Belt
        };

    }
}
