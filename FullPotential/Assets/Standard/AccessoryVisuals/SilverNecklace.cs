using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Accessories;

namespace FullPotential.Standard.AccessoryVisuals
{
    public class SilverNecklace : IAccessoryVisuals
    {
        private static readonly Guid Id = new Guid("e02761e5-5155-4b61-8f1c-8feb240a420c");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Accessories/Amulet.prefab";

        public string ApplicableToTypeIdString => Amulet.TypeIdString;
    }
}
