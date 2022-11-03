using System;
using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Standard.Weapons
{
    public class Hammer : IGearWeapon
    {
        public Guid TypeId => new Guid("70d38942-f1a9-4bd9-a3a4-c75e8615e31a");

        public string TypeName => nameof(Hammer);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Hammer.prefab";

        public string PrefabAddressTwoHanded => "Standard/Prefabs/Weapons/Hammer2.prefab";
    }
}
