using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Weapons
{
    public class Sword : IGearWeapon
    {
        public Guid TypeId => new Guid("b1ff5c3c-a306-4a2a-9fef-24320e05e74f");

        public string TypeName => nameof(Sword);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Sword.prefab";

        //todo: different prefab for two-handed
        public string PrefabAddressTwoHanded => "Standard/Prefabs/Sword.prefab";
    }
}
