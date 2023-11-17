using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Core.Registry.Armor
{
    public class Feet : IArmor
    {
        public Guid TypeId => ArmorTypeIds.Feet;

        public string TypeName => nameof(Feet);

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/Feet.png";
    }
}
