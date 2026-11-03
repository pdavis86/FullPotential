using System;

using FullPotential.Api.Registry;
using FullPotential.Standard.CustomGear.Barrier;

namespace FullPotential.Standard.CustomGearVisuals
{
    public class BasicWard : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("c2dbfd42-9a5b-4b0b-ba90-6b02ab710859");

        public string PrefabAddress => "Standard/Prefabs/Armor/Barrier.prefab";

        public string ApplicableToTypeIdString => Barrier.TypeIdString;
    }
}
