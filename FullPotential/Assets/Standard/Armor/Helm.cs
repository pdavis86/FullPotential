using System;

using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Helm : IArmorType
    {
        public const string TypeIdString = "bd6f655b-6fed-42e5-8797-e9cb3f675696";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Helm.png";
    }
}
