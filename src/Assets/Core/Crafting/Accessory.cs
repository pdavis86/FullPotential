using Assets.ApiScripts.Crafting;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Assets.Core.Crafting
{
    [System.Serializable]
    public class Accessory : GearBase, ICraftableAccessory
    {
        public string TypeName { get; set; }

        public ICraftable.CraftingCategory Category { get; set; }





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
