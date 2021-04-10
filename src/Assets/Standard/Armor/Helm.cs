using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Armor
{
    public class Helm : IGearArmor
    {
        public string TypeName => "Helm";

        public IGearArmor.ArmorSlots InventorySlot => IGearArmor.ArmorSlots.Helm;
    }
}
