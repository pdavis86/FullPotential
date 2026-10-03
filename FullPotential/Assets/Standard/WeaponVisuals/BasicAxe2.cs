using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicAxe2 : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("1245711e-f1e3-40d9-95be-0eb2cd420884");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Axe2.prefab";

        public string ApplicableToTypeIdString => Axe.TypeIdString;
    }
}
