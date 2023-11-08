using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class SilverNecklace : IAccessoryVisuals
    {
        public Guid TypeId => new Guid("ddeafb61-0163-4888-b355-16a37d3a33b5");

        public string TypeName => nameof(SilverNecklace);

        public AccessoryType Type => AccessoryType.Amulet;

        public string PrefabAddress => "Standard/Prefabs/Accessories/Amulet.prefab";
    }
}
