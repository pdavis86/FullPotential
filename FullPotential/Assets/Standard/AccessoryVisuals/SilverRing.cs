using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Accessories;

namespace FullPotential.Standard.AccessoryVisuals
{
    public class SilverRing : IAccessoryVisuals
    {
        private static readonly Guid Id = new Guid("c70a9495-0ef7-48fb-9b16-aad5fe7b29ad");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Accessories/Ring.prefab";

        public string ApplicableToTypeIdString => Ring.TypeIdString;
    }
}
