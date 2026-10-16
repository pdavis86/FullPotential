using System;

using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Legs : IArmorType
    {
        public const string TypeIdString = "b4ec0616-8a9c-4052-a318-482a305bb263";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Legs.png";
    }
}
