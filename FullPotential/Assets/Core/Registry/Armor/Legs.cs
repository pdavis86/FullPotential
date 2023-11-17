using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Core.Registry.Armor
{
    public class Legs : IArmor
    {
        public Guid TypeId => ArmorTypeIds.Legs;

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/Legs.png";
    }
}
