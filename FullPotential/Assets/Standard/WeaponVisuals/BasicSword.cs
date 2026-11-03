using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicSword : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("6cc25369-bc33-44c7-b168-2e38844c5713");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Sword.prefab";

        public string ApplicableToTypeIdString => Sword.TypeIdString;
    }
}
