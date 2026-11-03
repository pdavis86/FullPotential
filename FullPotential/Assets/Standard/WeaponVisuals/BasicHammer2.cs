using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicHammer2 : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("04fef0e9-2a84-42bb-96fa-9a3410d3c0d1");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Hammer2.prefab";

        public string ApplicableToTypeIdString => Hammer.TypeIdString;
    }
}
