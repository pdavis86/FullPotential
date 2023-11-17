using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicGun2 : IWeaponVisuals
    {
        public Guid TypeId => new Guid("afa3e371-9fd0-4081-ba5f-bf2c922cd87d");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Gun2.prefab";

        public Guid ApplicableToTypeId => new Guid(Gun.TypeIdString);
    }
}
