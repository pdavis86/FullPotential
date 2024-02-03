using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.SpecialGear.Barrier;

namespace FullPotential.Standard.SpecialGearVisuals
{
    public class BasicWard : ISpecialGearVisuals
    {
        public Guid TypeId => new Guid("c2dbfd42-9a5b-4b0b-ba90-6b02ab710859");

        public string PrefabAddress => "Standard/Prefabs/Armor/Barrier.prefab";

        public Guid ApplicableToTypeId => new Guid(Barrier.TypeIdString);
    }
}
