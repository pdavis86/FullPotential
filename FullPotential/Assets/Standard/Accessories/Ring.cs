using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Accessories
{
    public class Ring : IGearAccessory
    {
        public Guid TypeId => new Guid("b74b00f9-9cf1-4758-9e22-b4fbd4d1cea0");

        public string TypeName => nameof(Ring);

        public IGearAccessory.AccessoryCategory Category => IGearAccessory.AccessoryCategory.Ring;

        //todo: missing prefab
        public string PrefabAddress => throw new NotImplementedException();
    }
}
