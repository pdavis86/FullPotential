using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Helm : IArmor
    {
        private static readonly Guid Id = new Guid(ArmorTypeIds.HelmId);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Helm.png";
    }
}
