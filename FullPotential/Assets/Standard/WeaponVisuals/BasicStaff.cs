using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicStaff : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("c3e6ba00-4ea2-4e67-afdf-85e2bd3c21cb");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Staff.prefab";

        public string ApplicableToTypeIdString => Staff.TypeIdString;
    }
}
