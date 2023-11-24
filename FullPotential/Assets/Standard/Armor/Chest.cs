using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Chest : IArmor
    {
        public Guid TypeId => ArmorTypeIds.Chest;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Chest.png";
    }
}
