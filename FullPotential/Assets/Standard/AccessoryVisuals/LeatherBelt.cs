using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Accessories;

namespace FullPotential.Standard.AccessoryVisuals
{
    public class LeatherBelt : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("3275abb7-fc48-4682-abc1-85dda7abf24e");

        public string PrefabAddress => "Standard/Prefabs/Accessories/Belt.prefab";

        public string ApplicableToTypeIdString => Belt.TypeIdString;
    }
}
