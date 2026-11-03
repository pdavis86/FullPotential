using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicBow : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("3ee7395d-39d0-485c-a2e7-c01db9589941");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Bow.prefab";

        public string ApplicableToTypeIdString => Bow.TypeIdString;
    }
}
