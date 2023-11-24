using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicDagger : IWeaponVisuals
    {
        public Guid TypeId => new Guid("9c52dc28-4a47-4164-bd22-f83967dd3c22");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Dagger.prefab";

        public Guid ApplicableToTypeId => new Guid(Dagger.TypeIdString);
    }
}
