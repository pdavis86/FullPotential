using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicBow : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("3ee7395d-39d0-485c-a2e7-c01db9589941");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Bow.prefab";

        public string ApplicableToTypeIdString => Bow.TypeIdString;
    }
}
