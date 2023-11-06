using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Bow : IWeapon
    {
        public Guid TypeId => new Guid("47d23976-45ad-4360-b603-7ea4ed29846b");

        public string TypeName => nameof(Bow);

        public bool IsDefensive => false;

        public Guid? AmmunitionTypeId => new Guid(Ammo.Arrow.Id);

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;

        public string PrefabAddress => null;

        public string PrefabAddressTwoHanded => "Standard/Prefabs/Weapons/Bow.prefab";
    }
}
