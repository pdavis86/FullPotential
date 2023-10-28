using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class LeatherBelt : IAccessoryVisuals
    {
        public Guid TypeId => new Guid("6d4bce60-dda6-4a88-82fd-c2b086065c8b");

        public string TypeName => nameof(LeatherBelt);

        public AccessoryCategory Category => AccessoryCategory.Belt;

        public string PrefabAddress => "Standard/Prefabs/Accessories/Belt.prefab";
    }
}
