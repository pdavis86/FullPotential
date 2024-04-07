using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicSword : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("6cc25369-bc33-44c7-b168-2e38844c5713");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Sword.prefab";

        public string ApplicableToTypeIdString => Sword.TypeIdString;
    }
}
