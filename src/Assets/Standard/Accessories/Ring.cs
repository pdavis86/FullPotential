using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Accessories
{
    public class Ring : IGearAccessory
    {
        public string TypeName => "Ring";

        public IGearAccessory.AccessorySlots InventorySlot => IGearAccessory.AccessorySlots.Ring;
    }
}
