using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Ring : IGearAccessory
    {
        public Guid TypeId => new Guid("b74b00f9-9cf1-4758-9e22-b4fbd4d1cea0");

        public string TypeName => nameof(Ring);

        public IGearAccessory.AccessoryCategory Category => IGearAccessory.AccessoryCategory.Ring;

        public string PrefabAddress => "Standard/Prefabs/Accessories/Ring.prefab";
    }
}
