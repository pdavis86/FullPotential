using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Accessories
{
    public class Belt : IGearAccessory
    {
        public string TypeName => "Belt";

        public IGearAccessory.AccessorySlots InventorySlot => IGearAccessory.AccessorySlots.Belt;
    }
}
