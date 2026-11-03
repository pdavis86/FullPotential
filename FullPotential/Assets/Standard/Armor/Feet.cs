using System;

using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Feet : IArmorType
    {
        public const string TypeIdString = "645b2a9b-02df-4fb7-bea2-9b7a2a9c620b";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Feet.png";
    }
}
