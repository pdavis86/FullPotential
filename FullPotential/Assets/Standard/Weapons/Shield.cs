using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Weapons
{
    public class Shield : IGearWeapon
    {
        public Guid TypeId => new Guid("2b0d5e47-77b0-4311-98ee-0e41827f5fc4");

        public string TypeName => nameof(Shield);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Defensive;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => false;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Shield.prefab";

        public string PrefabAddressTwoHanded => null;
    }
}
