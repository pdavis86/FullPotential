using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Belt : IAccessoryType
    {
        public const string TypeIdString = "6d4bce60-dda6-4a88-82fd-c2b086065c8b";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public int SlotCount => 1;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Belt.png";
    }
}
