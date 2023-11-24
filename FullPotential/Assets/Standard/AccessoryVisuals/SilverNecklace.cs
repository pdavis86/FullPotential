using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Accessories;

namespace FullPotential.Standard.AccessoryVisuals
{
    public class SilverNecklace : IAccessoryVisuals
    {
        public Guid TypeId => new Guid("e02761e5-5155-4b61-8f1c-8feb240a420c");

        public string PrefabAddress => "Standard/Prefabs/Accessories/Amulet.prefab";

        public Guid ApplicableToTypeId => new Guid(Amulet.TypeIdString);
    }
}
