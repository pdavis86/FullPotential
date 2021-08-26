using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Weapons
{
    public class Dagger : IGearWeapon
    {
        public Guid TypeId => new Guid("6eabecda-d308-48b2-b7d3-93c7800df8c2");

        public string TypeName => nameof(Dagger);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => false;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Dagger.prefab";

        public string PrefabAddressTwoHanded => null;
    }
}
