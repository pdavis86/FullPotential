using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Accessories
{
    public class Amulet : IGearAccessory
    {
        public Guid TypeId => new Guid("ddeafb61-0163-4888-b355-16a37d3a33b5");

        public string TypeName => nameof(Amulet);

        public IGearAccessory.AccessoryCategory Category => IGearAccessory.AccessoryCategory.Amulet;

        //todo: missing prefab
        public string PrefabAddress => throw new NotImplementedException();
    }
}
