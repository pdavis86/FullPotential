using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicSword2 : IWeaponVisuals
    {
        public Guid TypeId => new Guid("ae8a2891-fd10-4c48-abd1-cd3361fc0bc2");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Sword2.prefab";

        public Guid ApplicableToTypeId => new Guid(Sword.TypeIdString);
    }
}
