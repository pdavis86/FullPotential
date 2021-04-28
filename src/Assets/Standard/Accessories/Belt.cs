using Assets.ApiScripts.Registry;
using System;

namespace Assets.Standard.Accessories
{
    public class Belt : IGearAccessory
    {
        public Guid TypeId => new Guid("6d4bce60-dda6-4a88-82fd-c2b086065c8b");

        public string TypeName => nameof(Belt);

        public IGearAccessory.AccessorySlot InventorySlot => IGearAccessory.AccessorySlot.Belt;
    }
}
