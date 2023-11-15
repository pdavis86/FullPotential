using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Armor;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherJerkin : IArmorVisuals
    {
        public Guid TypeId => new Guid("a2989e18-6830-4770-8695-1c8592137e2d");

        public string TypeName => nameof(LeatherJerkin);

        public string PrefabAddress => "Standard/Prefabs/Armor/Chest.prefab";

        public Guid ApplicableToTypeId => new Guid(Chest.TypeIdString);
    }
}
