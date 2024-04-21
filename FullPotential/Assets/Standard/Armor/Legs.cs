using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Legs : IArmor
    {
        private static readonly Guid Id = new Guid(ArmorTypeIds.LegsId);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Legs.png";
    }
}
