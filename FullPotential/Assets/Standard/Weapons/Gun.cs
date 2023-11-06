using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Gun : IWeapon
    {
        public Guid TypeId => new Guid("9b5a211a-07d2-4e5c-b8b8-639dbfb807e9");

        public string TypeName => nameof(Gun);

        public bool IsDefensive => false;

        public Guid? AmmunitionTypeId => new Guid(Ammo.Bullet.Id);

        public bool AllowAutomatic => true;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Gun.prefab";

        public string PrefabAddressTwoHanded => "Standard/Prefabs/Weapons/Gun2.prefab";
    }
}
