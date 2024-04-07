using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicSword2 : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("ae8a2891-fd10-4c48-abd1-cd3361fc0bc2");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Sword2.prefab";

        public string ApplicableToTypeIdString => Sword.TypeIdString;
    }
}
