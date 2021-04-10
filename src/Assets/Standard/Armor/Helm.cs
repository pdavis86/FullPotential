using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Armor
{
    public class Helm : IGearArmor
    {
        public Guid TypeId => new Guid("bd6f655b-6fed-42e5-8797-e9cb3f675696");

        public string TypeName => nameof(Helm);

        public IGearArmor.ArmorSlot InventorySlot => IGearArmor.ArmorSlot.Helm;
    }
}
