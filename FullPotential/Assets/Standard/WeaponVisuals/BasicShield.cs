using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicShield : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("d2e35462-acb4-41bc-940b-2fe0324cfac6");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Shield.prefab";

        public string ApplicableToTypeIdString => Shield.TypeIdString;
    }
}
