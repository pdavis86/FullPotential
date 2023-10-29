using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Dagger : IWeapon
    {
        public Guid TypeId => new Guid("6eabecda-d308-48b2-b7d3-93c7800df8c2");

        public string TypeName => nameof(Dagger);

        public WeaponCategory Category => WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => false;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Dagger.prefab";

        public string PrefabAddressTwoHanded => null;
    }
}
