using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicSword2 : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("ae8a2891-fd10-4c48-abd1-cd3361fc0bc2");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Sword2.prefab";

        public string ApplicableToTypeIdString => Sword.TypeIdString;
    }
}
