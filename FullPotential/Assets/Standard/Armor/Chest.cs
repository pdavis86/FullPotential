using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Chest : IArmor
    {
        private static readonly Guid Id = new Guid(ArmorTypeIds.ChestId);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Chest.png";
    }
}
