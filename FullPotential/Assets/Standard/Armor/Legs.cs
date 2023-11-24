using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Legs : IArmor
    {
        public Guid TypeId => ArmorTypeIds.Legs;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Legs.png";
    }
}
