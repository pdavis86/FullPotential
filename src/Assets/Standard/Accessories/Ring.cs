using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Accessories
{
    public class Ring : IGearAccessory
    {
        public Guid TypeId => new Guid("b74b00f9-9cf1-4758-9e22-b4fbd4d1cea0");

        public string TypeName => nameof(Ring);

        public IGearAccessory.AccessorySlot InventorySlot => IGearAccessory.AccessorySlot.Ring;
    }
}
