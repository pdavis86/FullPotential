using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.SpecialGear.Barrier;

namespace FullPotential.Standard.SpecialGearVisuals
{
    public class BasicWard : ISpecialGearVisuals
    {
        private static readonly Guid Id = new Guid("c2dbfd42-9a5b-4b0b-ba90-6b02ab710859");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Armor/Barrier.prefab";

        public string ApplicableToTypeIdString => Barrier.TypeIdString;
    }
}
