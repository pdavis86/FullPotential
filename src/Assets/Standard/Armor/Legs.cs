using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Armor
{
    public class Legs : IGearArmor
    {
        public string TypeName => "Legs";

        public IGearArmor.ArmorSlots InventorySlot => IGearArmor.ArmorSlots.Legs;
    }
}
