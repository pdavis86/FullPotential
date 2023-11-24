using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicCrossbow : IWeaponVisuals
    {
        public Guid TypeId => new Guid("e599c546-2b9d-4e29-9fe2-abfa09eaeb91");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Crossbow.prefab";

        public Guid ApplicableToTypeId => new Guid(Crossbow.TypeIdString);
    }
}
