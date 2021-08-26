using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Weapons
{
    public class Hammer : IGearWeapon
    {
        public Guid TypeId => new Guid("70d38942-f1a9-4bd9-a3a4-c75e8615e31a");

        public string TypeName => nameof(Hammer);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Hammer.prefab";

        //todo: different prefab for two-handed
        public string PrefabAddressTwoHanded => "Standard/Prefabs/Hammer.prefab";
    }
}
