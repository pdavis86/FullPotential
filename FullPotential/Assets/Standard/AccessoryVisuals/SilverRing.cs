using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Accessories;

namespace FullPotential.Standard.AccessoryVisuals
{
    public class SilverRing : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("c70a9495-0ef7-48fb-9b16-aad5fe7b29ad");

        public string PrefabAddress => "Standard/Prefabs/Accessories/Ring.prefab";

        public string ApplicableToTypeIdString => Ring.TypeIdString;
    }
}
