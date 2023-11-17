using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherJerkin : IArmorVisuals
    {
        public Guid TypeId => new Guid("a2989e18-6830-4770-8695-1c8592137e2d");

        public string PrefabAddress => "Standard/Prefabs/Armor/Chest.prefab";

        public Guid ApplicableToTypeId => ArmorTypeIds.Chest;
    }
}
