using System;

using FullPotential.Api.Registry;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicStaff : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("c3e6ba00-4ea2-4e67-afdf-85e2bd3c21cb");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Staff.prefab";

        public string ApplicableToTypeIdString => Staff.TypeIdString;
    }
}
