using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Ring : IAccessory
    {
        public const string TypeIdString = "b74b00f9-9cf1-4758-9e22-b4fbd4d1cea0";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public int SlotCount => 2;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Ring.png";
    }
}
