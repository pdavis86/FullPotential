using System;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Standard.Weapons
{
    public class Sword : IWeapon
    {
        public Guid TypeId => new Guid("b1ff5c3c-a306-4a2a-9fef-24320e05e74f");

        public string TypeName => nameof(Sword);

        public bool IsDefensive => false;

        public Guid? AmmunitionTypeId => null;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Sword.prefab";

        public string PrefabAddressTwoHanded => "Standard/Prefabs/Weapons/Sword2.prefab";
    }
}
