using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicShield : IWeaponVisuals
    {
        public Guid TypeId => new Guid("d2e35462-acb4-41bc-940b-2fe0324cfac6");


        public string PrefabAddress => "Standard/Prefabs/Weapons/Shield.prefab";

        public Guid ApplicableToTypeId => new Guid(Shield.TypeIdString);
    }
}
