using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Standard.Accessories
{
    public class Ring : IGearAccessory
    {
        public Guid TypeId => new Guid("b74b00f9-9cf1-4758-9e22-b4fbd4d1cea0");

        public string TypeName => nameof(Ring);

        public AccessoryCategory Category => AccessoryCategory.Ring;

        public string PrefabAddress => "Standard/Prefabs/Accessories/Ring.prefab";
    }
}
