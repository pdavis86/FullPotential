using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Amulet : IGearAccessory
    {
        public Guid TypeId => new Guid("ddeafb61-0163-4888-b355-16a37d3a33b5");

        public string TypeName => nameof(Amulet);

        public IGearAccessory.AccessoryCategory Category => IGearAccessory.AccessoryCategory.Amulet;

        public string PrefabAddress => "Standard/Prefabs/Accessories/Amulet.prefab";
    }
}
