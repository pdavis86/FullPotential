using System;

using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Chest : IArmorType
    {
        public const string TypeIdString = "2419fcac-217e-48d8-9770-76c5ff27c9f8";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Chest.png";
    }
}
