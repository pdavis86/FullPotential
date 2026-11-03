using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicShield : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("d2e35462-acb4-41bc-940b-2fe0324cfac6");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Shield.prefab";

        public string ApplicableToTypeIdString => Shield.TypeIdString;
    }
}
