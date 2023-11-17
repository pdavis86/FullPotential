using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Core.Registry.Armor
{
    public class Chest : IArmor
    {
        public Guid TypeId => ArmorTypeIds.Chest;

        public string TypeName => nameof(Chest);

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/Chest.png";
    }
}
