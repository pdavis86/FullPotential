using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicCrossbow : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("e599c546-2b9d-4e29-9fe2-abfa09eaeb91");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Crossbow.prefab";

        public string ApplicableToTypeIdString => Crossbow.TypeIdString;
    }
}
