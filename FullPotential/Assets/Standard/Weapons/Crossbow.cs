using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Crossbow : IWeapon
    {
        public Guid TypeId => new Guid("3d8be950-b8b0-44c6-ab84-1bf8434d67bd");

        public string TypeName => nameof(Crossbow);

        public bool IsDefensive => false;

        public Guid? AmmunitionTypeId => new Guid(Ammo.Arrow.Id);

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;

        public string PrefabAddress => null;

        public string PrefabAddressTwoHanded => "Standard/Prefabs/Weapons/Crossbow.prefab";
    }
}
