using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Assets.Scripts.Crafting.Results
{
    public class Accessory : GearBase
    {
        public const string Amulet = "Amulet";
        public const string Ring = "Ring";
        public const string Belt = "Belt";

        public static readonly List<string> AccessoryOptions = new List<string>
        {
            Amulet,
            Ring,
            Belt
        };

    }
}
