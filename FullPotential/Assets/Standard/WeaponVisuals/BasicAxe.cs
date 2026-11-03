using System;

using FullPotential.Api.Registry.Visuals;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicAxe : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("ac8a55d8-6304-47d6-a35d-eecb0319d1ad");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Axe.prefab";

        public string ApplicableToTypeIdString => Axe.TypeIdString;
    }
}
