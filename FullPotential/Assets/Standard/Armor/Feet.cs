using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Feet : IArmor
    {
        public Guid TypeId => ArmorTypeIds.Feet;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Feet.png";
    }
}
