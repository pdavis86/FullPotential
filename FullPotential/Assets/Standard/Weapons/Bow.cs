using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Weapons
{
    public class Bow : IGearWeapon
    {
        public Guid TypeId => new Guid("47d23976-45ad-4360-b603-7ea4ed29846b");

        public string TypeName => nameof(Bow);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;

        public string PrefabAddress => null;

        public string PrefabAddressTwoHanded => "Standard/Prefabs/Bow.prefab";
    }
}
