using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Accessories
{
    public class Belt : IGearAccessory
    {
        public Guid TypeId => new Guid("6d4bce60-dda6-4a88-82fd-c2b086065c8b");

        public string TypeName => nameof(Belt);

        public IGearAccessory.AccessoryCategory Category => IGearAccessory.AccessoryCategory.Belt;

        //todo: missing prefab
        public string PrefabAddress => throw new NotImplementedException();
    }
}
