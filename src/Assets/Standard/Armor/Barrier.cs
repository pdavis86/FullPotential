using Assets.ApiScripts.Crafting;

namespace Assets.Standard.Armor
{
    public class Barrier : IGearArmor
    {
        public string TypeName => "Barrier";

        public IGearArmor.ArmorSlots InventorySlot => IGearArmor.ArmorSlots.Barrier;
    }
}
