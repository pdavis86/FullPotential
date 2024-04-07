using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicHammer2 : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("04fef0e9-2a84-42bb-96fa-9a3410d3c0d1");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Hammer2.prefab";

        public string ApplicableToTypeIdString => Hammer.TypeIdString;
    }
}
