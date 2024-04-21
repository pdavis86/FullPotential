using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Feet : IArmor
    {
        private static readonly Guid Id = new Guid(ArmorTypeIds.FeetId);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Feet.png";
    }
}
