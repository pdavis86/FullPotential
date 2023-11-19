using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Helm : IArmor
    {
        public Guid TypeId => ArmorTypeIds.Helm;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Helm.png";
    }
}
