using System.Collections.Generic;

namespace Assets.Scripts.Ui.Crafting.Items
{
    public class Accessory : ItemBase
    {
        public const string Amulet = "Amulet";
        public const string Ring = "Ring";
        public const string Belt = "Belt";

        public static List<string> AccessoryOptions = new List<string> {
            Amulet,
            Ring,
            Belt
        };

    }
}
