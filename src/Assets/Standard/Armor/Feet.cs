using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Armor
{
    public class Feet : IGearArmor
    {
        public string TypeName => "Feet";

        public IGearArmor.ArmorSlots InventorySlot => IGearArmor.ArmorSlots.Feet;
    }
}
