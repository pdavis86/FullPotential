using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicGun : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("5e495402-176e-4727-9dd1-e63b6044ab1f");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Gun.prefab";

        public string ApplicableToTypeIdString => Gun.TypeIdString;
    }
}
