using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicGun : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("5e495402-176e-4727-9dd1-e63b6044ab1f");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Gun.prefab";

        public string ApplicableToTypeIdString => Gun.TypeIdString;
    }
}
