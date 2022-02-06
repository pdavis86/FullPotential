using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Weapons
{
    public class Axe : IGearWeapon
    {
        public Guid TypeId => new Guid("0bef1fe6-4b04-4700-bd51-6ff82a10703b");

        public string TypeName => nameof(Axe);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Axe.prefab";

        public string PrefabAddressTwoHanded => "Standard/Prefabs/Weapons/Axe2.prefab";
    }
}
