using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Core.Registry.Armor
{
    public class Helm : IArmor
    {
        public Guid TypeId => ArmorTypeIds.Helm;

        public string TypeName => nameof(Helm);

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/Helm.png";
    }
}
