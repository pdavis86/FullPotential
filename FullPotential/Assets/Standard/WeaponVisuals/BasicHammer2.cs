using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicHammer2 : IWeaponVisuals
    {
        public Guid TypeId => new Guid("04fef0e9-2a84-42bb-96fa-9a3410d3c0d1");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Hammer2.prefab";

        public Guid ApplicableToTypeId => new Guid(Hammer.TypeIdString);
    }
}
