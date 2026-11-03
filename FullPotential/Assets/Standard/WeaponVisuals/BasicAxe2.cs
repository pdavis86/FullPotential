using System;

using FullPotential.Api.Registry;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicAxe2 : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("1245711e-f1e3-40d9-95be-0eb2cd420884");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Axe2.prefab";

        public string ApplicableToTypeIdString => Axe.TypeIdString;
    }
}
