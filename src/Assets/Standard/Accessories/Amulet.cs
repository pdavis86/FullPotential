using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Accessories
{
    public class Amulet : IGearAccessory
    {
        public string TypeName => "Amulet";

        public IGearAccessory.AccessorySlots InventorySlot => IGearAccessory.AccessorySlots.Amulet;
    }
}
