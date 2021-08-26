using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Weapons
{
    public class Axe : IGearWeapon
    {
        public Guid TypeId => new Guid("0bef1fe6-4b04-4700-bd51-6ff82a10703b");

        public string TypeName => nameof(Axe);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Melee;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Axe.prefab";

        //todo: different prefab for two-handed
        public string PrefabAddressTwoHanded => "Standard/Prefabs/Axe.prefab";
    }
}
