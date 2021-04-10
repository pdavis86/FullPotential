using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Armor
{
    public class Chest : IGearArmor
    {
        public string TypeName => "Chest";

        public IGearArmor.ArmorSlots InventorySlot => IGearArmor.ArmorSlots.Chest;
    }
}
